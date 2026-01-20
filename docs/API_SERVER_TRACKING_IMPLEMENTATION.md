# API Implementation: Server Tracking & Health Monitoring

## 🎯 Overview

MatchZy Enhanced now sends a `server_configured` event whenever a CS2 server is configured with your webhook URL. This enables your API to track active servers, monitor their health, and maintain a server status dashboard.

---

## 📥 New Event: `server_configured`

### When This Event is Sent

1. **Immediately** when admin runs: `matchzy_remote_log_url "https://your-api.com/events"`
2. **On server startup** (5 second delay) if webhook URL is already configured
3. **After server restart/crash** - automatic re-registration

### Event Payload

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

### Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| `event` | string | Always `"server_configured"` |
| `server_id` | string | Unique server identifier (from `matchzy_server_id`) |
| `hostname` | string | Server hostname (from `hostname` convar) |
| `plugin_version` | string | MatchZy Enhanced version (e.g., "1.3.6") |
| `remote_log_url` | string | The webhook URL configured on this server |
| `timestamp` | number | Unix timestamp when event was sent |
| `configured_by` | string | Either `"Console"` (manual config) or `"Startup"` (auto on boot) |

---

## 💾 Database Schema

### Recommended Table Structure

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

### Alternative: Add to Existing Table

If you already have a `servers` or `cs2_servers` table, just add these columns:

```sql
ALTER TABLE servers ADD COLUMN plugin_version VARCHAR(50);
ALTER TABLE servers ADD COLUMN last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE servers ADD COLUMN status VARCHAR(50) DEFAULT 'configured';
```

---

## 🔧 Implementation Steps

### Step 1: Handle `server_configured` Event

Add this to your existing webhook event handler:

```typescript
// Your existing event endpoint
app.post('/api/events/:serverId', async (req, res) => {
  try {
    const event = req.body;
    const serverId = req.params.serverId || event.server_id;
    
    // NEW: Handle server_configured event
    if (event.event === 'server_configured') {
      await handleServerConfigured(event);
      return res.status(200).json({ 
        success: true, 
        message: 'Server registered successfully' 
      });
    }
    
    // Update last_seen for ANY event (heartbeat)
    await updateServerLastSeen(serverId);
    
    // Your existing event handlers
    if (event.event === 'match_end') {
      await handleMatchEnd(event);
    }
    // ... other handlers
    
    res.status(200).json({ success: true });
    
  } catch (error) {
    console.error('Event processing error:', error);
    res.status(500).json({ error: 'Internal server error' });
  }
});
```

### Step 2: Implement `handleServerConfigured`

```typescript
async function handleServerConfigured(event: any) {
  const {
    server_id,
    hostname,
    plugin_version,
    remote_log_url,
    timestamp,
    configured_by
  } = event;
  
  // Upsert server record
  await db.servers.upsert({
    where: { server_id },
    update: {
      hostname,
      plugin_version,
      webhook_url: remote_log_url,
      last_seen: new Date(timestamp * 1000),
      status: 'online',
      updated_at: new Date()
    },
    create: {
      server_id,
      hostname,
      plugin_version,
      webhook_url: remote_log_url,
      last_seen: new Date(timestamp * 1000),
      status: 'online',
      created_at: new Date()
    }
  });
  
  console.log(`✓ Server registered: ${server_id} (${hostname}) - ${configured_by}`);
  
  // Optional: Log to audit trail
  await db.audit_log.create({
    event_type: 'server_configured',
    server_id,
    message: `Server ${hostname} configured via ${configured_by}`,
    timestamp: new Date()
  });
}
```

### Step 3: Implement Heartbeat System

Update `last_seen` timestamp for **every** event:

```typescript
async function updateServerLastSeen(serverId: string) {
  try {
    await db.servers.update(
      { server_id: serverId },
      { 
        last_seen: new Date(),
        status: 'online'
      }
    );
  } catch (error) {
    console.error(`Failed to update last_seen for ${serverId}:`, error);
  }
}
```

### Step 4: Health Monitoring Cron Job

Add a cron job to mark inactive servers as offline:

```typescript
import cron from 'node-cron';

// Run every 1 minute
cron.schedule('* * * * *', async () => {
  try {
    const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
    
    // Find servers that haven't sent events in 5+ minutes
    const inactiveServers = await db.servers.findMany({
      where: {
        last_seen: { lt: fiveMinutesAgo },
        status: { not: 'offline' }
      }
    });
    
    if (inactiveServers.length > 0) {
      // Mark as offline
      await db.servers.updateMany(
        {
          where: {
            server_id: { in: inactiveServers.map(s => s.server_id) }
          }
        },
        { status: 'offline' }
      );
      
      console.log(`⚠️  Marked ${inactiveServers.length} server(s) as offline`);
      
      // Optional: Send alerts for critical servers
      const criticalOffline = inactiveServers.filter(s => s.is_critical);
      if (criticalOffline.length > 0) {
        await sendSlackAlert(
          `🚨 ${criticalOffline.length} critical server(s) offline!`,
          criticalOffline.map(s => `- ${s.hostname} (${s.server_id})`).join('\n')
        );
      }
    }
  } catch (error) {
    console.error('Health check cron error:', error);
  }
});
```

---

## 📊 Dashboard API Endpoints

### Get All Servers with Status

```typescript
app.get('/api/servers', async (req, res) => {
  try {
    const servers = await db.servers.findAll({
      orderBy: { last_seen: 'desc' }
    });
    
    const now = new Date();
    
    const serversWithStatus = servers.map(server => {
      const lastSeenMinutes = Math.floor(
        (now.getTime() - server.last_seen.getTime()) / 1000 / 60
      );
      
      return {
        serverId: server.server_id,
        hostname: server.hostname,
        pluginVersion: server.plugin_version,
        status: lastSeenMinutes < 5 ? 'online' : 'offline',
        lastSeen: server.last_seen,
        lastSeenMinutes,
        webhookUrl: server.webhook_url
      };
    });
    
    res.json({
      total: servers.length,
      online: serversWithStatus.filter(s => s.status === 'online').length,
      offline: serversWithStatus.filter(s => s.status === 'offline').length,
      servers: serversWithStatus
    });
  } catch (error) {
    console.error('Failed to fetch servers:', error);
    res.status(500).json({ error: 'Failed to fetch servers' });
  }
});
```

### Get Server Statistics

```typescript
app.get('/api/servers/stats', async (req, res) => {
  try {
    const total = await db.servers.count();
    
    const online = await db.servers.count({
      where: { status: 'online' }
    });
    
    const offline = await db.servers.count({
      where: { status: 'offline' }
    });
    
    // Version distribution
    const versions = await db.servers.groupBy({
      by: ['plugin_version'],
      _count: true
    });
    
    res.json({
      total,
      online,
      offline,
      versions: versions.map(v => ({
        version: v.plugin_version,
        count: v._count
      }))
    });
  } catch (error) {
    console.error('Failed to fetch server stats:', error);
    res.status(500).json({ error: 'Failed to fetch stats' });
  }
});
```

### Get Individual Server Details

```typescript
app.get('/api/servers/:serverId', async (req, res) => {
  try {
    const { serverId } = req.params;
    
    const server = await db.servers.findUnique({
      where: { server_id: serverId }
    });
    
    if (!server) {
      return res.status(404).json({ error: 'Server not found' });
    }
    
    const now = new Date();
    const lastSeenMinutes = Math.floor(
      (now.getTime() - server.last_seen.getTime()) / 1000 / 60
    );
    
    res.json({
      ...server,
      status: lastSeenMinutes < 5 ? 'online' : 'offline',
      lastSeenMinutes
    });
  } catch (error) {
    console.error('Failed to fetch server:', error);
    res.status(500).json({ error: 'Failed to fetch server' });
  }
});
```

---

## 🎨 Frontend Integration Examples

### Server Status Badge

```typescript
function ServerStatusBadge({ server }: { server: Server }) {
  const isOnline = server.lastSeenMinutes < 5;
  
  return (
    <div className={`badge ${isOnline ? 'badge-success' : 'badge-error'}`}>
      <div className="indicator">
        <div className={`dot ${isOnline ? 'dot-online' : 'dot-offline'}`} />
        {isOnline ? 'Online' : 'Offline'}
      </div>
      <span className="text-xs">
        {server.lastSeenMinutes}m ago
      </span>
    </div>
  );
}
```

### Server List Component

```typescript
function ServerList() {
  const { data: servers } = useQuery('/api/servers');
  
  return (
    <div className="grid gap-4">
      <div className="stats">
        <div className="stat">
          <div className="stat-title">Total Servers</div>
          <div className="stat-value">{servers.total}</div>
        </div>
        <div className="stat">
          <div className="stat-title">Online</div>
          <div className="stat-value text-success">{servers.online}</div>
        </div>
        <div className="stat">
          <div className="stat-title">Offline</div>
          <div className="stat-value text-error">{servers.offline}</div>
        </div>
      </div>
      
      <div className="table">
        {servers.servers.map(server => (
          <div key={server.serverId} className="table-row">
            <div>{server.hostname}</div>
            <div>{server.serverId}</div>
            <div><ServerStatusBadge server={server} /></div>
            <div>{server.pluginVersion}</div>
          </div>
        ))}
      </div>
    </div>
  );
}
```

---

## 🧪 Testing

### Test Server Registration

```bash
# On CS2 server console
matchzy_server_id "test-server-01"
matchzy_remote_log_url "https://your-api.com/events"

# Check your API logs for:
# ✓ Server registered: test-server-01 (Your Server Name) - Console
```

### Test Heartbeat System

```bash
# Play a match and check database
# last_seen should update with every event

# In your API, query:
SELECT server_id, hostname, last_seen, status 
FROM servers 
ORDER BY last_seen DESC;
```

### Test Health Monitoring

```bash
# Stop a CS2 server
# Wait 6 minutes
# Check database - server should be marked offline

# Or manually trigger health check:
curl http://localhost:3000/api/servers
```

---

## ✅ Checklist

- [ ] Add `servers` table to database schema
- [ ] Implement `handleServerConfigured` function
- [ ] Add `server_configured` event handler to webhook endpoint
- [ ] Implement `updateServerLastSeen` heartbeat function
- [ ] Add health monitoring cron job (runs every minute)
- [ ] Create `/api/servers` endpoint
- [ ] Create `/api/servers/stats` endpoint
- [ ] Create `/api/servers/:serverId` endpoint
- [ ] Update frontend to display server status
- [ ] Test server registration on CS2 server
- [ ] Verify heartbeat updates `last_seen`
- [ ] Verify offline detection after 5+ minutes

---

## 🚀 Quick Start (Minimal Implementation)

If you want the absolute minimum to get started:

```typescript
// 1. Add to your event handler
if (event.event === 'server_configured') {
  await db.servers.upsert({
    where: { server_id: event.server_id },
    update: {
      hostname: event.hostname,
      plugin_version: event.plugin_version,
      last_seen: new Date(),
      status: 'online'
    },
    create: {
      server_id: event.server_id,
      hostname: event.hostname,
      plugin_version: event.plugin_version,
      last_seen: new Date(),
      status: 'online'
    }
  });
  
  console.log(`✓ Server registered: ${event.server_id}`);
  return res.status(200).json({ success: true });
}

// 2. Update last_seen for all events
await db.servers.update(
  { server_id: event.server_id },
  { last_seen: new Date(), status: 'online' }
);
```

**That's it!** You'll now have server tracking. Add the cron job and dashboard endpoints when ready.

---

## 📞 Support

If you need help implementing this:
1. Check your event logs for `server_configured` events
2. Verify database schema matches above
3. Test with a single CS2 server first
4. Check API logs for registration confirmations

The MatchZy side is fully implemented and ready. Just implement these API handlers and you'll have full server tracking! 🎉
