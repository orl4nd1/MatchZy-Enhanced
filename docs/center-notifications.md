# Center HTML Notifications

MatchZy Enhanced can display important match events in the center of players' screens for better visibility and a professional tournament experience.

## Configuration

```cfg
matchzy_center_html_notifications "1"  // 1 = enabled (default), 0 = disabled
```

Add this to `cfg/MatchZy/config.cfg` or set via RCON/console.

---

## Features

### 🌍 Global Notifications (All Players)

Displayed to everyone on the server:

**Match Live**
```
🔴 MATCH LIVE 🔴
Team1 vs Team2
```
Large green notification when the match goes live.

**Pause**
```
⏸️ PAUSED ⏸️
Team Name
```
Yellow notification showing which team paused.

**Unpause**
```
▶️ RESUMING ▶️
```
Green notification when match resumes.

**Knife Round Winner**
```
🔪 Team Name WON KNIFE
Waiting for side selection...
```
Orange notification after knife round.

**Side Selection**
```
🔪 Team Name STAYS
🔪 Team Name SWITCHES
```
Green notification when side is chosen.

---

### 👤 Personal Notifications (Individual Players)

Shown only to specific players:

**Ready Status**
```
✅ YOU ARE READY
```
Green confirmation when player types `.ready`.

**Not Ready**
```
❌ NOT READY
Type .ready to ready up
```
Red notification with instructions when player types `.unready`.

**Auto-Ready**
```
✅ AUTO-READY
Type .unready to opt-out
```
Green notification when auto-ready is enabled.

---

### 👥 Team-Specific Notifications

Shown only to relevant team members:

**Unpause Confirmation**
```
⏸️ Team X WANTS TO UNPAUSE
Type .unpause to confirm
```
Orange notification asking the other team to confirm unpause.

---

### ⏱️ Countdown Timers

Live second-by-second countdowns for:

**Pause Duration** (if limit configured)
```
⏸️ PAUSE AUTO-ENDS IN 300s
⏸️ PAUSE AUTO-ENDS IN 299s
⏸️ PAUSE AUTO-ENDS IN 298s
...
```
Yellow countdown showing remaining pause time.

**Side Selection Timer**
```
🔪 Team Name SIDE SELECTION
60s remaining
```
Orange countdown after knife round.

**Server Restart**
```
⚠️ SERVER RESTART
30s
```
Red countdown before server kicks all players.

---

## Benefits

✅ **Better Visibility** — Important messages can't be missed  
✅ **Less Chat Spam** — Clean interface, chat remains readable  
✅ **Professional Look** — Tournament-style notifications  
✅ **Clear Instructions** — Players know exactly what to do  
✅ **Real-Time Updates** — Live countdown timers  
✅ **Team Coordination** — Team-specific messages reduce confusion

---

## Technical Details

### Bot Exclusion
- Automatically excludes bots to avoid console spam
- Only real players see notifications

### Works With Chat
- Center notifications complement chat messages
- Chat messages still appear for players who prefer them
- Full localization support maintained

### Performance
- Lightweight HTML rendering
- No impact on server performance
- Updates only when needed (countdowns update every second)

---

## Use Cases

### Competitive Leagues
- Players never miss pause notifications
- Clear countdown timers for pause limits
- Professional tournament feel

### Public Servers
- New players get clear instructions (`.ready`, `.unpause`)
- Auto-ready notifications explain the system
- Less confusion about match state

### Fast-Paced Tournaments
- Quick visual feedback for all actions
- Countdown timers show urgency
- Server restart warnings give players time to react

---

## Troubleshooting

### Notifications not showing?

Check:
```cfg
matchzy_center_html_notifications "1"  // Make sure it's enabled
```

### Want to disable for specific match?

Set via match config JSON:
```json
{
  "cvars": {
    "matchzy_center_html_notifications": 0
  }
}
```

Or via console:
```
matchzy_center_html_notifications 0
```

---

## Examples

### Tournament Match Flow

1. **Players connect** → Personal ready status shown
2. **Match live** → Big center announcement
3. **Team pauses** → Pause notification + countdown (if limit set)
4. **Other team wants to unpause** → Team-specific message
5. **Match resumes** → Resume notification
6. **Knife round ends** → Winner shown + side selection countdown
7. **Side chosen** → Choice confirmed to all players
8. **Series ends** → Server restart countdown

### Configuration Combinations

**Maximum visibility (default):**
```cfg
matchzy_center_html_notifications "1"
matchzy_pause_duration "300"
matchzy_side_selection_time "60"
```
All notifications enabled with countdown timers.

**Chat only (minimal):**
```cfg
matchzy_center_html_notifications "0"
```
Disables all center notifications, uses chat only.

**Hybrid (selective):**
```cfg
matchzy_center_html_notifications "1"
matchzy_pause_duration "0"  // No pause countdown
matchzy_side_selection_time "0"  // No side selection countdown
```
Center notifications without countdown timers.

---

## Related Settings

- `matchzy_chat_prefix` — Chat message prefix
- `matchzy_pause_duration` — Enables pause countdown
- `matchzy_side_selection_time` — Enables side selection countdown
- `matchzy_autoready_enabled` — Triggers auto-ready notifications
- `matchzy_both_teams_unpause_required` — Triggers unpause confirmation messages

---

## Version

Added in **MatchZy Enhanced v1.4.7**

---

**[← Back to Documentation](https://me.sivert.io/)**
