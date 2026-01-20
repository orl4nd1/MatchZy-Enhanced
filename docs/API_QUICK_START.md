# 🚨 URGENT: New Event for API Implementation

## What Changed?

MatchZy Enhanced now sends a **`server_configured`** event when CS2 servers connect to your API.

---

## 📥 New Event You'll Receive

```json
{
  "event": "server_configured",
  "server_id": "prod-server-01",
  "hostname": "Tournament Server #1",
  "plugin_version": "1.3.6",
  "remote_log_url": "https://api.example.com/events",
  "timestamp": 1737333120,
  "configured_by": "Console"
}
```

**When sent:**
- Immediately when server is configured with your webhook URL
- On server startup (if already configured)
- After server restart/crash

---

## ⚡ Quick Implementation (5 minutes)

### 1. Add Handler to Your Event Endpoint

```typescript
// In your existing POST /api/events/:serverId endpoint
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
  
  console.log(`✓ Server registered: ${event.server_id} (${event.hostname})`);
  return res.status(200).json({ success: true });
}
```

### 2. Add Database Table

```sql
CREATE TABLE servers (
  server_id VARCHAR(255) PRIMARY KEY,
  hostname VARCHAR(255) NOT NULL,
  plugin_version VARCHAR(50),
  last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  status VARCHAR(50) DEFAULT 'online',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_servers_last_seen ON servers(last_seen);
```

### 3. Add Heartbeat (Optional but Recommended)

Update `last_seen` for **every** event:

```typescript
// After handling any event
await db.servers.update(
  { server_id: event.server_id },
  { last_seen: new Date(), status: 'online' }
);
```

---

## 🎯 What You Get

✅ Automatic server registration  
✅ Track which servers are active  
✅ Monitor server health (online/offline)  
✅ Version tracking  
✅ Dashboard-ready data  

---

## 📚 Full Documentation

See `API_SERVER_TRACKING_IMPLEMENTATION.md` for:
- Complete database schema
- Health monitoring cron job
- Dashboard endpoints
- Frontend examples
- Testing procedures

---

## 🧪 Test It

1. On CS2 server: `matchzy_remote_log_url "https://your-api.com/events"`
2. Check your API logs: `✓ Server registered: ...`
3. Check database: `SELECT * FROM servers;`

---

## ⏰ Timeline

- **Minimum:** Add event handler (5 minutes)
- **Recommended:** Add heartbeat + health checks (15 minutes)
- **Optional:** Dashboard endpoints (30 minutes)

---

## 🆘 Need Help?

The event is **already being sent** from all MatchZy servers. Just add the handler and you're good to go!
