# MatchZy Enhanced v1.3.0 - Match Creation Guide

> **For Tournament Platforms & Automated Match Creation Systems**

This guide covers the new configuration options added in v1.3.0 that tournament platforms and match creation systems should be aware of when creating matches.

---

## 🎯 Quick Overview

MatchZy Enhanced v1.3.0 adds **6 new configurable features** to improve player experience and match flow. All features are **disabled by default** for safety and must be explicitly enabled.

---

## ⚙️ Configuration Reference

### 1. Auto-Ready System

**What it does:** Players are automatically marked as ready when they join the server.

**Configuration:**
```cfg
matchzy_autoready_enabled "0"  // 0 = disabled (default), 1 = enabled
```

**When to use:**
- ✅ Fast-paced tournaments where players are expected to be ready immediately
- ✅ Scheduled matches where join time = start time
- ❌ Casual matches where players need setup time

**Notes:**
- Players can still type `.unready` if they need more time
- Match starts automatically when minimum players are ready

---

### 2. Enhanced Pause System

**What it does:** Adds limits and timeouts to pausing.

**Configuration:**
```cfg
matchzy_both_teams_unpause_required "1"  // 0 = single team can unpause, 1 = both teams required (default)
matchzy_max_pauses_per_team "0"          // 0 = unlimited (default), or set limit (e.g., 2)
matchzy_pause_duration "0"               // 0 = unlimited (default), or seconds (e.g., 300 for 5 min)
```

**When to use:**
- `both_teams_unpause_required "1"`: Most competitive matches (prevents troll unpauses)
- `max_pauses_per_team "2"`: Tournament matches to prevent pause abuse
- `pause_duration "300"`: Ranked matches to prevent indefinite pauses

**Examples:**
```cfg
// Competitive tournament
matchzy_both_teams_unpause_required "1"
matchzy_max_pauses_per_team "2"
matchzy_pause_duration "300"

// Casual scrim
matchzy_both_teams_unpause_required "0"
matchzy_max_pauses_per_team "0"
matchzy_pause_duration "0"
```

---

### 3. Side Selection Timer

**What it does:** Adds a countdown timer after knife round for side selection.

**Configuration:**
```cfg
matchzy_side_selection_enabled "1"   // 0 = disabled, 1 = enabled (default)
matchzy_side_selection_time "60"     // Seconds before random selection (default: 60)
```

**When to use:**
- ✅ Most tournaments (prevents indefinite waiting)
- ❌ Only disable if you have external side selection logic

**Notes:**
- If timer expires, a random side (stay/swap) is chosen automatically
- Players can still use `.ct`, `.t`, `.stay`, or `.swap` commands before timer expires

**Recommended values:**
```cfg
// Standard tournament
matchzy_side_selection_time "60"

// Fast tournament
matchzy_side_selection_time "30"

// Extended time for coordination
matchzy_side_selection_time "90"
```

---

### 4. Early Match Termination (.gg Command)

**What it does:** Allows teams to forfeit/surrender by voting.

**Configuration:**
```cfg
matchzy_gg_enabled "0"       // 0 = disabled (default), 1 = enabled
matchzy_gg_threshold "0.8"   // Percentage of team required (default: 0.8 = 80%)
```

**When to use:**
- ✅ Scrims and practice matches
- ✅ Unranked tournaments where forfeits are acceptable
- ❌ Ranked matches or official tournaments (keep disabled)

**How it works:**
- Players type `.gg` to vote
- When threshold is reached (e.g., 4 out of 5 players = 80%), match ends immediately
- Opposing team wins automatically
- Votes reset each round

**Examples:**
```cfg
// Practice/scrim server
matchzy_gg_enabled "1"
matchzy_gg_threshold "0.8"

// Official tournament (keep disabled)
matchzy_gg_enabled "0"
```

---

### 5. FFW (Forfeit/Walkover) System

**What it does:** Automatically forfeits if an entire team disconnects.

**Configuration:**
```cfg
matchzy_ffw_enabled "0"      // 0 = disabled (default), 1 = enabled
matchzy_ffw_time "240"       // Seconds before forfeit (default: 240 = 4 minutes)
```

**When to use:**
- ✅ Online tournaments with connection issues
- ✅ Automated tournament platforms
- ❌ LAN tournaments (keep disabled)

**How it works:**
- Timer starts when all players from one team disconnect
- Chat warnings every minute
- If any player returns, timer cancels automatically
- If timer expires, opposing team wins by forfeit

**Examples:**
```cfg
// Online tournament
matchzy_ffw_enabled "1"
matchzy_ffw_time "240"  // 4 minutes

// Quick forfeit for fast brackets
matchzy_ffw_enabled "1"
matchzy_ffw_time "120"  // 2 minutes

// LAN tournament (disabled)
matchzy_ffw_enabled "0"
```

---

## 🎮 Command Reference

### New Commands in v1.3.0

| Command | Description | Feature Required |
|---------|-------------|------------------|
| `.gg` | Vote to forfeit match | `matchzy_gg_enabled "1"` |
| `.up` | Alias for `.unpause` | Always available |
| `.p` | Alias for `.pause` | Always available |

### Existing Commands (Still Available)

All existing MatchZy commands remain unchanged:
- `.ready` / `.unready`
- `.pause` / `.unpause` (now also `.p` / `.up`)
- `.ct` / `.t` / `.stay` / `.swap` (now with timer)
- `.coach` / `.uncoach`
- Practice mode commands

---

## 📋 Recommended Configurations by Match Type

### 🏆 Competitive Tournament (Official)
```cfg
matchzy_autoready_enabled "0"                    # Let players ready manually
matchzy_both_teams_unpause_required "1"          # Both teams must unpause
matchzy_max_pauses_per_team "2"                  # 2 pauses per team
matchzy_pause_duration "300"                     # 5 minute pause limit
matchzy_side_selection_enabled "1"               # Enable timer
matchzy_side_selection_time "60"                 # 60 second timer
matchzy_gg_enabled "0"                           # Disable forfeits
matchzy_ffw_enabled "1"                          # Enable walkover
matchzy_ffw_time "240"                           # 4 minute walkover timer
```

### ⚡ Fast-Paced Tournament (Quick Matches)
```cfg
matchzy_autoready_enabled "1"                    # Auto-ready on join
matchzy_both_teams_unpause_required "1"          # Both teams must unpause
matchzy_max_pauses_per_team "1"                  # 1 pause per team
matchzy_pause_duration "180"                     # 3 minute pause limit
matchzy_side_selection_enabled "1"               # Enable timer
matchzy_side_selection_time "30"                 # 30 second timer
matchzy_gg_enabled "0"                           # Disable forfeits
matchzy_ffw_enabled "1"                          # Enable walkover
matchzy_ffw_time "120"                           # 2 minute walkover timer
```

### 🎯 Ranked/Matchmaking
```cfg
matchzy_autoready_enabled "0"                    # Manual ready
matchzy_both_teams_unpause_required "1"          # Both teams must unpause
matchzy_max_pauses_per_team "2"                  # 2 pauses per team
matchzy_pause_duration "300"                     # 5 minute pause limit
matchzy_side_selection_enabled "1"               # Enable timer
matchzy_side_selection_time "45"                 # 45 second timer
matchzy_gg_enabled "1"                           # Enable surrenders
matchzy_gg_threshold "0.8"                       # 80% team vote required
matchzy_ffw_enabled "1"                          # Enable walkover
matchzy_ffw_time "180"                           # 3 minute walkover timer
```

### 🏋️ Practice/Scrim
```cfg
matchzy_autoready_enabled "1"                    # Auto-ready for quick start
matchzy_both_teams_unpause_required "0"          # Single team can unpause
matchzy_max_pauses_per_team "0"                  # Unlimited pauses
matchzy_pause_duration "0"                       # No pause limit
matchzy_side_selection_enabled "1"               # Enable timer
matchzy_side_selection_time "60"                 # 60 second timer
matchzy_gg_enabled "1"                           # Allow forfeits
matchzy_gg_threshold "0.6"                       # 60% vote (easier forfeit)
matchzy_ffw_enabled "0"                          # Disabled (casual)
```

### 🧪 Testing/Development
```cfg
matchzy_autoready_enabled "1"                    # Auto-ready
matchzy_both_teams_unpause_required "0"          # Single team unpause
matchzy_max_pauses_per_team "0"                  # Unlimited
matchzy_pause_duration "0"                       # Unlimited
matchzy_side_selection_enabled "0"               # Disabled for testing
matchzy_gg_enabled "1"                           # Enable for quick exits
matchzy_gg_threshold "0.5"                       # 50% (very easy)
matchzy_ffw_enabled "0"                          # Disabled
```

---

## 🔄 Match JSON Configuration

These settings can also be configured per-match via the match JSON file. ConVars set in the JSON will override server defaults for that specific match.

**Example match JSON:**
```json
{
  "matchid": "match_12345",
  "num_maps": 1,
  "team1": { "name": "Team A" },
  "team2": { "name": "Team B" },
  "cvars": {
    "matchzy_autoready_enabled": 1,
    "matchzy_both_teams_unpause_required": 1,
    "matchzy_max_pauses_per_team": 2,
    "matchzy_pause_duration": 300,
    "matchzy_side_selection_enabled": 1,
    "matchzy_side_selection_time": 60,
    "matchzy_gg_enabled": 0,
    "matchzy_ffw_enabled": 1,
    "matchzy_ffw_time": 240
  }
}
```

---

## 🚨 Important Notes

### Default Values (Safe Defaults)
All new features are **disabled by default** or set to **unlimited/permissive** values:
- Auto-ready: **Disabled**
- Max pauses: **Unlimited**
- Pause duration: **Unlimited**
- Side selection: **Enabled with 60s timer**
- .gg command: **Disabled**
- FFW system: **Disabled**

### Backward Compatibility
- ✅ All existing match configurations work without changes
- ✅ All original commands still function identically
- ✅ No breaking changes to match JSON format

### Best Practices
1. **Start conservative**: Use default values for first matches
2. **Test thoroughly**: Test new settings in scrims before tournaments
3. **Communicate rules**: Inform players of pause limits, forfeit options, etc.
4. **Monitor feedback**: Adjust timers/limits based on player feedback
5. **Document settings**: Save successful configurations for reuse

---

## 📊 Decision Matrix

**Should I enable auto-ready?**
- Yes if: Matches are scheduled and players expect to start immediately
- No if: Players need time to configure settings/warm up

**Should I limit pauses?**
- Yes if: Competitive tournament with strict rules
- No if: Casual play or practice

**Should I enable .gg?**
- Yes if: Unranked/casual matches where quick exits are acceptable
- No if: Official tournaments where all rounds matter

**Should I enable FFW?**
- Yes if: Online tournaments where connection issues occur
- No if: LAN tournaments or reliable connections

---

## 🆘 Support

For questions or issues with these features:
- GitHub: [MatchZy Enhanced Issues](https://github.com/sivert-io/MatchZy-Enhanced/issues)
- Documentation: [Full configuration guide](configuration.md)
- Testing: [Test scenarios](testing-guide.md)

---

**Last Updated:** January 19, 2026 (v1.3.0)
