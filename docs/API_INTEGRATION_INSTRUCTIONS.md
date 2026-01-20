# API Integration Instructions for Event-Driven Architecture

## Overview

MatchZy Enhanced now implements a **bulletproof event delivery system** with:
- ✅ **Automatic retry** with exponential backoff
- ✅ **Local event queue** survives API downtime
- ✅ **Pull API** for missing data recovery
- ✅ **No database exposure** - HTTP-only communication

---

## Architecture

```
CS2 Server (MatchZy)           →  Tournament API          →  Central Database
┌──────────────────────┐          ┌──────────────────┐       ┌──────────────┐
│ 1. Event happens     │          │ 1. Receive event │       │ PostgreSQL/  │
│ 2. Store in local DB │ ─POST──▶ │ 2. Validate      │ ────▶ │ MySQL        │
│ 3. Send to API       │          │ 3. Store         │       │              │
│                      │          │ 4. Broadcast WS  │       │ Source of    │
│ If failed:           │          │                  │       │ Truth        │
│ 4. Queue for retry   │          │ If data missing: │       └──────────────┘
│ 5. Retry every 30s   │ ◀─GET─── │ 5. Pull from CS2 │
│    (exponential)     │          │    server        │
└──────────────────────┘          └──────────────────┘
      Local SQLite/MySQL              REST API + WebSocket
```

---

## What MatchZy Now Does (Server-Side)

### 1. **Event Queue Table**

```sql
CREATE TABLE matchzy_event_queue (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    event_type TEXT NOT NULL,           -- 'round_end', 'match_end', etc.
    event_data TEXT NOT NULL,           -- Full JSON payload
    match_id INTEGER,                   -- Match identifier
    map_number INTEGER DEFAULT 0,       -- Map number
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    retry_count INTEGER DEFAULT 0,      -- Retry attempts
    last_retry DATETIME,                -- Last retry timestamp
    next_retry DATETIME,                -- Next scheduled retry
    status TEXT DEFAULT 'pending',      -- 'pending', 'sent', 'failed'
    error_message TEXT                  -- Error details
);
```

### 2. **Automatic Event Queueing**

When an event POST fails (non-2xx status or exception):
- Event is saved to `matchzy_event_queue` table
- Scheduled for retry in 30 seconds
- Server continues operating normally

### 3. **Retry Background Process**

- Runs every 30 seconds
- Processes pending events with exponential backoff:
  - Attempt 1: 30 seconds
  - Attempt 2: 1 minute
  - Attempt 3: 2 minutes
  - Attempt 4: 4 minutes
  - Attempt 5: 8 minutes
  - Attempt 6: 16 minutes
  - Attempt 7+: 32 minutes (capped)
- Maximum 20 retries before marking as `failed`
- Auto-cleanup of successfully sent events after 7 days

### 4. **Pull API Commands**

New console commands for data recovery:

#### `matchzy_get_match_stats <matchId>`
Returns complete match statistics as JSON:
```json
{
  "match": {
    "matchid": 123,
    "start_time": "2026-01-20 14:00:00",
    "end_time": "2026-01-20 15:30:00",
    "winner": "Team Alpha",
    "series_type": "bo3",
    "team1_name": "Team Alpha",
    "team1_score": 2,
    "team2_name": "Team Beta",
    "team2_score": 1
  },
  "maps": [
    {
      "matchid": 123,
      "mapnumber": 0,
      "mapname": "de_inferno",
      "winner": "Team Alpha",
      "team1_score": 16,
      "team2_score": 14,
      "start_time": "2026-01-20 14:00:00",
      "end_time": "2026-01-20 14:45:00"
    }
  ],
  "players": [
    {
      "matchid": 123,
      "mapnumber": 0,
      "steamid64": 76561198938147909,
      "team": "Team Alpha",
      "name": "Player1",
      "kills": 25,
      "deaths": 18,
      "assists": 5,
      "damage": 2500,
      "headshot_kills": 12,
      "enemy3ks": 2,
      "v1_wins": 3,
      "utility_damage": 250
      // ... all player stats
    }
  ]
}
```

#### `matchzy_get_pending_events`
Shows event queue status (admin only):
```
Event queue status:
  Pending events: 15
  Breakdown:
    - round_end: 8
    - player_death: 5
    - match_end: 2
```

---

## What You Need to Implement (API-Side)

### **Special Event: `server_configured`**

When a server is configured with your webhook URL (or on startup if already configured), MatchZy sends a `server_configured` event:

```json
{
  "event": "server_configured",
  "server_id": "prod-server-01",
  "hostname": "Tournament Server #1",
  "plugin_version": "1.3.6",
  "remote_log_url": "https://api.example.com/events/server-01",
  "timestamp": 1737333120,
  "configured_by": "Console"
}
```

**Use this to:**
- Track which servers are active and configured
- Maintain a "server status" table in your database
- Verify webhook connectivity
- Display server health in your dashboard

**Example handler:**
```typescript
if (event.event === 'server_configured') {
  await db.servers.upsert({
    serverId: event.server_id,
    hostname: event.hostname,
    pluginVersion: event.plugin_version,
    webhookUrl: event.remote_log_url,
    lastSeen: new Date(event.timestamp * 1000),
    status: 'configured'
  });
  
  console.log(`✓ Server registered: ${event.server_id} (${event.hostname})`);
  
  return res.status(200).json({ success: true, message: 'Server registered' });
}
```

**When this event is sent:**
1. When admin runs `matchzy_remote_log_url "https://api.example.com/events"` → Immediate
2. On server startup (5 second delay) if webhook URL already configured → Automatic
3. After server crash/restart → Automatic on boot

**Database schema suggestion:**
```sql
CREATE TABLE servers (
  server_id VARCHAR(255) PRIMARY KEY,
  hostname VARCHAR(255) NOT NULL,
  plugin_version VARCHAR(50),
  webhook_url TEXT,
  last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  status VARCHAR(50) DEFAULT 'configured',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE INDEX idx_servers_status ON servers(status);
CREATE INDEX idx_servers_last_seen ON servers(last_seen);
```

**Dashboard queries:**
```typescript
// Get all active servers (seen in last 5 minutes)
const activeServers = await db.servers.findMany({
  where: {
    lastSeen: { gte: new Date(Date.now() - 5 * 60 * 1000) }
  }
});

// Get offline servers (not seen in 10 minutes)
const offlineServers = await db.servers.findMany({
  where: {
    lastSeen: { lt: new Date(Date.now() - 10 * 60 * 1000) }
  }
});

// Check version distribution
const versionStats = await db.servers.groupBy({
  by: ['plugin_version'],
  _count: true
});
```

---

### **Option 1: Simple Approach (Recommended for MVP)**

Just keep your existing webhook receiver. MatchZy will handle retries automatically!

```typescript
// Existing endpoint - no changes needed!
app.post('/api/events/:serverId', async (req, res) => {
  try {
    const event = req.body;
    
    // Validate event
    if (!event.event || !event.matchid) {
      return res.status(400).json({ error: 'Invalid event' });
    }
    
    // Store in database
    await db.events.create({
      matchId: event.matchid,
      eventType: event.event,
      eventData: event,
      receivedAt: new Date()
    });
    
    // Broadcast to WebSocket clients
    wss.clients.forEach(client => {
      client.send(JSON.stringify({
        type: 'match_event',
        matchId: event.matchid,
        event: event
      }));
    });
    
    // Return 200 OK so MatchZy marks event as sent
    res.status(200).json({ success: true });
    
  } catch (error) {
    console.error('Event processing error:', error);
    // Return 500 so MatchZy queues for retry
    res.status(500).json({ error: 'Internal error' });
  }
});
```

**That's it!** MatchZy handles all retries. If your API is down for 10 minutes, when it comes back up, all queued events will be delivered automatically.

---

### **Option 2: Advanced Approach (Pull Missing Data)**

Add a "pull endpoint" for when you detect missing data:

#### 1. **Detect Missing Data**

```typescript
app.get('/api/matches/:matchId/stats', async (req, res) => {
  const { matchId } = req.params;
  
  // Check if we have this match in our DB
  const match = await db.matches.findOne({ matchId });
  
  if (!match || match.status === 'incomplete') {
    // Data is missing! Try to pull from server
    const serverUrl = await getServerUrlForMatch(matchId);
    
    if (serverUrl) {
      try {
        const stats = await pullStatsFromServer(serverUrl, matchId);
        
        if (stats) {
          // Store the pulled data
          await storeMatchStats(stats);
          return res.json(stats);
        }
      } catch (error) {
        console.error('Failed to pull stats from server:', error);
      }
    }
    
    return res.status(404).json({ error: 'Match data not available' });
  }
  
  // We have the data, return it
  res.json(match);
});
```

#### 2. **Pull Stats from Server**

```typescript
async function pullStatsFromServer(serverUrl: string, matchId: number) {
  try {
    // Use RCON or HTTP to execute command on CS2 server
    // (You'll need to implement RCON connection or HTTP endpoint)
    
    // Example with RCON:
    const rcon = new RCON(serverUrl, rconPassword);
    await rcon.connect();
    
    const response = await rcon.command(`matchzy_get_match_stats ${matchId}`);
    
    // Parse the JSON from console output
    // Format: [GetMatchStats] Match ID 123 stats:\n{"match":...}\n
    const jsonMatch = response.match(/\[GetMatchStats\] Match ID \d+ stats:\s*(\{.*\})/s);
    
    if (jsonMatch && jsonMatch[1]) {
      return JSON.parse(jsonMatch[1]);
    }
    
    return null;
  } catch (error) {
    console.error('RCON pull error:', error);
    return null;
  }
}
```

#### 3. **Server URL Tracking**

Track which server is running which match:

```typescript
// When match starts, store server info
app.post('/api/matches/:matchId/started', async (req, res) => {
  const { matchId } = req.params;
  const { serverUrl, serverRcon } = req.body;
  
  await db.matches.update(
    { matchId },
    { serverUrl, serverRcon, status: 'live' }
  );
  
  res.json({ success: true });
});
```

---

## **Option 3: HTTP Endpoint on CS2 Server (Future Enhancement)**

If you want a direct HTTP endpoint instead of RCON, you could add to MatchZy:

```csharp
// Future enhancement - not implemented yet
[HttpGet("/matchzy/stats/{matchId}")]
public async Task<string> GetMatchStats(int matchId)
{
    var stats = database.GetMatchStatsJson(matchId);
    return stats ?? "{}";
}
```

Then pull directly via HTTP:
```typescript
const response = await fetch(`http://${serverUrl}:27020/matchzy/stats/${matchId}`);
const stats = await response.json();
```

---

## **Option 4: Server Heartbeat Monitoring (Optional)**

Use the `server_configured` event as a heartbeat for server health monitoring.

### **Basic Heartbeat System**

Update `last_seen` timestamp whenever **any** event is received:

```typescript
app.post('/api/events/:serverId', async (req, res) => {
  try {
    const event = req.body;
    const serverId = req.params.serverId || event.server_id;
    
    // Update last_seen for any event (heartbeat)
    await db.servers.update(
      { serverId },
      { lastSeen: new Date() }
    );
    
    // Handle specific events
    if (event.event === 'server_configured') {
      await handleServerConfigured(event);
    } else if (event.event === 'match_end') {
      await handleMatchEnd(event);
    }
    // ... other event handlers
    
    res.status(200).json({ success: true });
  } catch (error) {
    console.error('Event error:', error);
    res.status(500).json({ error: 'Internal error' });
  }
});
```

### **Health Check Cron Job**

Monitor server health every minute:

```typescript
// Run every 1 minute
cron.schedule('* * * * *', async () => {
  const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
  
  // Mark servers as offline if no events in 5 minutes
  await db.servers.updateMany(
    {
      where: {
        lastSeen: { lt: fiveMinutesAgo },
        status: { not: 'offline' }
      }
    },
    { status: 'offline' }
  );
  
  // Alert if critical servers are offline
  const offlineCritical = await db.servers.findMany({
    where: {
      status: 'offline',
      isCritical: true
    }
  });
  
  if (offlineCritical.length > 0) {
    await sendAlert(`${offlineCritical.length} critical servers offline!`);
  }
});
```

### **Dashboard Status Display**

```typescript
app.get('/api/servers/status', async (req, res) => {
  const now = new Date();
  const servers = await db.servers.findAll();
  
  const status = servers.map(server => {
    const lastSeenMinutes = Math.floor((now - server.lastSeen) / 1000 / 60);
    
    return {
      serverId: server.serverId,
      hostname: server.hostname,
      status: lastSeenMinutes < 5 ? 'online' : 'offline',
      lastSeen: server.lastSeen,
      lastSeenMinutes,
      pluginVersion: server.pluginVersion
    };
  });
  
  res.json({
    total: servers.length,
    online: status.filter(s => s.status === 'online').length,
    offline: status.filter(s => s.status === 'offline').length,
    servers: status
  });
});
```

---

## Error Handling Strategy

### **Your API Should:**

1. **Return 200 OK** when event is successfully received and stored
   - MatchZy marks event as `sent` and removes from queue

2. **Return 4xx** for invalid/malformed events
   - MatchZy will still retry (in case of temporary validation issues)
   - Consider return 200 with `{error: "validation"}` to prevent retries

3. **Return 5xx** for server errors
   - MatchZy will retry with exponential backoff
   - Perfect for database timeouts, temporary outages

4. **Timeout or connection refused**
   - MatchZy queues automatically
   - Retries when connection restored

---

## Benefits of This Architecture

✅ **Reliability**: No events lost even during API downtime  
✅ **Simplicity**: API just needs to return 200 OK  
✅ **Scalability**: Each server manages its own queue  
✅ **Security**: No database exposure, HTTP-only  
✅ **Flexibility**: Pull API for data recovery  
✅ **Visibility**: Query event queue status anytime  

---

## Testing the System

### **1. Test Normal Flow**
```bash
# Events should be sent immediately
tail -f /path/to/cs2/logs
# Look for: [MatchZy Events] ✓ 'round_end' sent successfully
```

### **2. Test Retry System**
```bash
# Stop your API temporarily
# Play a match
# Events will queue
rcon "matchzy_get_pending_events"
# Output: Pending events: 25

# Restart your API
# Wait 30 seconds
# Events will be retried automatically
# Check: Pending events: 0
```

### **3. Test Pull API**
```bash
# On CS2 server:
rcon "matchzy_get_match_stats 123"
# Returns full match stats as JSON
```

---

## Migration Guide

### **From Old System (No Retry)**

1. ✅ No API changes needed!
2. ✅ Just upgrade MatchZy Enhanced
3. ✅ Events will automatically retry on failure
4. ✅ Old events in flight will complete normally

### **From Shared Database**

If you currently share a database:

1. Keep existing event webhook endpoint
2. Add optional pull endpoint for recovery
3. Migrate servers to local SQLite/MySQL
4. Remove database credentials from servers
5. Close database ports to internet

---

## Questions to Consider

1. **Do you need the pull API?**
   - No: If retry system is sufficient (recommended for most)
   - Yes: If you need data recovery for long downtime periods

2. **How long should servers keep event history?**
   - Default: 7 days for sent events
   - Configurable via `CleanupOldEvents()` schedule

3. **Should failed events (20+ retries) be reported?**
   - Currently: Marked as `failed` in database
   - Optional: Add alerting endpoint to notify you

---

## Summary for Your LLM

**Minimum Required (MVP):**
- Keep existing `POST /api/events/:serverId` endpoint
- Return 200 OK when event stored successfully
- Return 5xx on errors
- Handle `server_configured` event to track active servers
- **Done!** MatchZy handles all retries

**Recommended Enhancements:**
- Use `server_configured` event to maintain server status database
- Update `last_seen` timestamp on every event for heartbeat monitoring
- Add health check cron job to mark servers offline after 5+ minutes
- Display server status in dashboard

**Optional Advanced Features:**
- Add `GET /api/matches/:matchId/stats` with pull fallback from CS2 server
- Track server URLs/RCON for data recovery
- Add monitoring/alerting for event queue sizes
- Version tracking and upgrade notifications

**No Changes Needed:**
- Database structure (same event schema)
- WebSocket broadcasting
- Frontend code

---

## Need Help?

The retry system is fully automated server-side. Your API just needs to:
1. Accept events
2. Return 200 OK
3. Everything else is handled by MatchZy Enhanced! 🚀
