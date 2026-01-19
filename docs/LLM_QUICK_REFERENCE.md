# MatchZy Enhanced v1.3.0 - LLM Configuration Reference

> **For AI systems and tournament platforms creating match configurations**

---

## Overview

MatchZy Enhanced v1.3.0 adds **10 new configuration variables** for enhanced match control. All features are **disabled by default** or set to unlimited/permissive values for safety.

---

## Configuration Variables Reference

### 1. Auto-Ready System

**ConVar:** `matchzy_autoready_enabled`  
**Type:** Boolean (`0` or `1`)  
**Default:** `0` (disabled)

**Behavior:**
- `0` = Players must manually type `.ready` (default)
- `1` = Players are automatically marked ready on connect

**When to enable:**
- Scheduled matches where join time = start time
- Fast-paced tournaments with minimal warmup
- Players are expected to be ready immediately

**When to disable:**
- Players need time for settings/warmup
- Casual/practice matches
- First-time players on server

---

### 2. Pause System (3 ConVars)

#### 2a. Both Teams Unpause Required

**ConVar:** `matchzy_both_teams_unpause_required`  
**Type:** Boolean (`0` or `1`)  
**Default:** `1` (enabled)

**Behavior:**
- `0` = Either team can unpause alone
- `1` = Both teams must type `.unpause` to resume (default)

**Recommendation:** Keep at `1` for competitive matches to prevent troll unpauses.

---

#### 2b. Max Pauses Per Team

**ConVar:** `matchzy_max_pauses_per_team`  
**Type:** Integer  
**Default:** `0` (unlimited)

**Behavior:**
- `0` = No limit on pauses (default)
- `1-999` = Maximum pauses allowed per team

**Common values:**
- `2` = Standard competitive (2 pauses per team)
- `1` = Fast tournaments
- `0` = Casual/practice (unlimited)

---

#### 2c. Pause Duration

**ConVar:** `matchzy_pause_duration`  
**Type:** Integer (seconds)  
**Default:** `0` (unlimited)

**Behavior:**
- `0` = No time limit on pauses (default)
- `1-999` = Maximum pause duration in seconds (auto-unpause after timeout)

**Common values:**
- `300` = 5 minutes (standard competitive)
- `180` = 3 minutes (fast tournaments)
- `600` = 10 minutes (extended time)
- `0` = Unlimited (casual/practice)

**Note:** Does NOT apply to admin pauses (always unlimited).

---

### 3. Side Selection Timer (2 ConVars)

#### 3a. Side Selection Enabled

**ConVar:** `matchzy_side_selection_enabled`  
**Type:** Boolean (`0` or `1`)  
**Default:** `1` (enabled)

**Behavior:**
- `0` = No timer (unlimited time to choose)
- `1` = Timer enabled (default)

**Recommendation:** Keep enabled to prevent indefinite waiting.

---

#### 3b. Side Selection Time

**ConVar:** `matchzy_side_selection_time`  
**Type:** Integer (seconds)  
**Default:** `60`

**Behavior:**
- After knife round, winning team has this many seconds to choose side
- If timer expires without choice, random side is selected
- Players use `.ct`, `.t`, `.stay`, or `.swap` commands

**Common values:**
- `30` = Fast tournaments
- `60` = Standard (default)
- `90` = Extended time for coordination

---

### 4. Early Match Termination (2 ConVars)

#### 4a. GG Enabled

**ConVar:** `matchzy_gg_enabled`  
**Type:** Boolean (`0` or `1`)  
**Default:** `0` (disabled)

**Behavior:**
- `0` = `.gg` command disabled (default)
- `1` = Teams can forfeit via `.gg` vote

**When to enable:**
- Scrims and practice matches
- Unranked tournaments
- Casual play

**When to disable:**
- Official/ranked tournaments
- Any match where forfeits are not acceptable

---

#### 4b. GG Threshold

**ConVar:** `matchzy_gg_threshold`  
**Type:** Float (0.0 to 1.0)  
**Default:** `0.8` (80%)

**Behavior:**
- Percentage of team required to vote `.gg` for forfeit
- Example: With 5 players and threshold `0.8`, need 4 votes (80% of 5)
- Votes reset every round

**Common values:**
- `0.8` = 80% (4/5 players) - standard
- `1.0` = 100% (all players) - unanimous
- `0.6` = 60% (3/5 players) - easier forfeit
- `0.5` = 50% (majority)

---

### 5. Forfeit/Walkover System (2 ConVars)

#### 5a. FFW Enabled

**ConVar:** `matchzy_ffw_enabled`  
**Type:** Boolean (`0` or `1`)  
**Default:** `0` (disabled)

**Behavior:**
- `0` = No automatic forfeit (default)
- `1` = Start forfeit timer when entire team disconnects

**When to enable:**
- Online tournaments (connection issues expected)
- Automated tournament platforms
- Matches where walkovers are acceptable

**When to disable:**
- LAN tournaments (no connection issues)
- Matches requiring manual admin intervention

---

#### 5b. FFW Time

**ConVar:** `matchzy_ffw_time`  
**Type:** Integer (seconds)  
**Default:** `240` (4 minutes)

**Behavior:**
- Time before forfeit is declared after entire team disconnects
- Countdown shows minute-by-minute warnings in chat
- Timer cancels if any team member returns

**Common values:**
- `240` = 4 minutes (default)
- `180` = 3 minutes (faster)
- `300` = 5 minutes (more lenient)
- `120` = 2 minutes (very fast)

---

## Configuration Templates

### Template 1: Official Competitive Tournament

**Use case:** High-stakes official matches, no forfeits allowed

```json
{
  "matchid": "official_12345",
  "cvars": {
    "matchzy_autoready_enabled": 0,
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

**Rationale:**
- Manual ready (players need warmup time)
- Both teams required to unpause (prevent trolling)
- 2 pauses max per team (standard competitive)
- 5 minute pause limit (prevents abuse)
- 60 second side selection (standard)
- `.gg` disabled (no forfeits in official matches)
- FFW enabled (handle connection issues fairly)

---

### Template 2: Fast-Paced Tournament

**Use case:** Multiple matches per day, quick turnaround

```json
{
  "matchid": "fast_67890",
  "cvars": {
    "matchzy_autoready_enabled": 1,
    "matchzy_both_teams_unpause_required": 1,
    "matchzy_max_pauses_per_team": 1,
    "matchzy_pause_duration": 180,
    "matchzy_side_selection_enabled": 1,
    "matchzy_side_selection_time": 30,
    "matchzy_gg_enabled": 0,
    "matchzy_ffw_enabled": 1,
    "matchzy_ffw_time": 120
  }
}
```

**Rationale:**
- Auto-ready (start immediately)
- 1 pause per team (minimize delays)
- 3 minute pause limit (fast-paced)
- 30 second side selection (quick decisions)
- `.gg` disabled (complete all matches)
- 2 minute FFW (fast walkover)

---

### Template 3: Ranked/Matchmaking

**Use case:** Skill-based matchmaking, allow surrenders

```json
{
  "matchid": "ranked_24680",
  "cvars": {
    "matchzy_autoready_enabled": 0,
    "matchzy_both_teams_unpause_required": 1,
    "matchzy_max_pauses_per_team": 2,
    "matchzy_pause_duration": 300,
    "matchzy_side_selection_enabled": 1,
    "matchzy_side_selection_time": 45,
    "matchzy_gg_enabled": 1,
    "matchzy_gg_threshold": 0.8,
    "matchzy_ffw_enabled": 1,
    "matchzy_ffw_time": 180
  }
}
```

**Rationale:**
- Manual ready (respect player setup time)
- 2 pauses, 5 minutes each (standard competitive)
- 45 second side selection (balanced)
- `.gg` enabled with 80% threshold (allow surrenders)
- 3 minute FFW (handle abandons)

---

### Template 4: Practice/Scrim

**Use case:** Team practice, casual play

```json
{
  "matchid": "scrim_13579",
  "cvars": {
    "matchzy_autoready_enabled": 1,
    "matchzy_both_teams_unpause_required": 0,
    "matchzy_max_pauses_per_team": 0,
    "matchzy_pause_duration": 0,
    "matchzy_side_selection_enabled": 1,
    "matchzy_side_selection_time": 60,
    "matchzy_gg_enabled": 1,
    "matchzy_gg_threshold": 0.6,
    "matchzy_ffw_enabled": 0
  }
}
```

**Rationale:**
- Auto-ready (quick start)
- Single team can unpause (casual)
- Unlimited pauses (practice environment)
- `.gg` enabled with lower threshold (easy forfeit)
- FFW disabled (not needed for practice)

---

### Template 5: Default/Safe Configuration

**Use case:** When uncertain, or testing

```json
{
  "matchid": "default_11111",
  "cvars": {
    "matchzy_autoready_enabled": 0,
    "matchzy_both_teams_unpause_required": 1,
    "matchzy_max_pauses_per_team": 0,
    "matchzy_pause_duration": 0,
    "matchzy_side_selection_enabled": 1,
    "matchzy_side_selection_time": 60,
    "matchzy_gg_enabled": 0,
    "matchzy_ffw_enabled": 0
  }
}
```

**Rationale:**
- Uses all default values (safest option)
- Manual ready, unlimited pauses
- No forfeits, no FFW
- Minimal restrictions

---

## Decision Logic for Match Configuration

### Step 1: Determine Match Type

```python
match_types = {
    "official_tournament": {
        "importance": "high",
        "allow_forfeit": False,
        "strict_rules": True
    },
    "fast_tournament": {
        "importance": "medium",
        "speed": "fast",
        "strict_rules": True
    },
    "ranked": {
        "importance": "medium",
        "allow_forfeit": True,
        "competitive": True
    },
    "scrim": {
        "importance": "low",
        "allow_forfeit": True,
        "casual": True
    }
}
```

### Step 2: Apply Configuration Rules

#### Rule 1: Auto-Ready
```python
if match_type in ["fast_tournament", "scrim"]:
    matchzy_autoready_enabled = 1
else:
    matchzy_autoready_enabled = 0  # Default: manual ready
```

#### Rule 2: Pause Limits
```python
if match_type == "official_tournament":
    matchzy_max_pauses_per_team = 2
    matchzy_pause_duration = 300
elif match_type == "fast_tournament":
    matchzy_max_pauses_per_team = 1
    matchzy_pause_duration = 180
elif match_type == "ranked":
    matchzy_max_pauses_per_team = 2
    matchzy_pause_duration = 300
else:  # scrim/practice
    matchzy_max_pauses_per_team = 0  # unlimited
    matchzy_pause_duration = 0  # unlimited
```

#### Rule 3: Side Selection Timer
```python
if match_type == "fast_tournament":
    matchzy_side_selection_time = 30  # quick
elif match_type == "ranked":
    matchzy_side_selection_time = 45  # balanced
else:
    matchzy_side_selection_time = 60  # standard
```

#### Rule 4: Forfeit (.gg) System
```python
if match_type == "official_tournament":
    matchzy_gg_enabled = 0  # NEVER allow forfeits
elif match_type in ["ranked", "scrim"]:
    matchzy_gg_enabled = 1
    if match_type == "scrim":
        matchzy_gg_threshold = 0.6  # easier (60%)
    else:
        matchzy_gg_threshold = 0.8  # standard (80%)
else:
    matchzy_gg_enabled = 0  # Default: disabled
```

#### Rule 5: FFW System
```python
if connection_type == "lan":
    matchzy_ffw_enabled = 0  # LAN has no connection issues
elif match_type in ["official_tournament", "ranked", "fast_tournament"]:
    matchzy_ffw_enabled = 1
    if match_type == "fast_tournament":
        matchzy_ffw_time = 120  # 2 minutes
    elif match_type == "ranked":
        matchzy_ffw_time = 180  # 3 minutes
    else:
        matchzy_ffw_time = 240  # 4 minutes (default)
else:
    matchzy_ffw_enabled = 0  # Casual: no FFW
```

### Step 3: Special Cases

#### High-Stakes Official Match
```python
config = {
    "matchzy_autoready_enabled": 0,          # Manual ready
    "matchzy_both_teams_unpause_required": 1,
    "matchzy_max_pauses_per_team": 2,
    "matchzy_pause_duration": 300,
    "matchzy_side_selection_time": 60,
    "matchzy_gg_enabled": 0,                 # NO forfeits
    "matchzy_ffw_enabled": 1,
    "matchzy_ffw_time": 300                  # 5 min (lenient)
}
```

#### Speed Run / Time Trial
```python
config = {
    "matchzy_autoready_enabled": 1,          # Instant start
    "matchzy_max_pauses_per_team": 0,        # No pauses allowed
    "matchzy_pause_duration": 0,
    "matchzy_side_selection_time": 15,       # Very fast
    "matchzy_gg_enabled": 0,
    "matchzy_ffw_enabled": 0
}
```

---

## Configuration Matrix

| Feature | Official | Fast | Ranked | Scrim | Default |
|---------|----------|------|--------|-------|---------|
| **Auto-Ready** | ❌ `0` | ✅ `1` | ❌ `0` | ✅ `1` | ❌ `0` |
| **Both Teams Unpause** | ✅ `1` | ✅ `1` | ✅ `1` | ❌ `0` | ✅ `1` |
| **Max Pauses/Team** | `2` | `1` | `2` | `0` | `0` |
| **Pause Duration** | `300` | `180` | `300` | `0` | `0` |
| **Side Selection Time** | `60` | `30` | `45` | `60` | `60` |
| **.gg Enabled** | ❌ `0` | ❌ `0` | ✅ `1` | ✅ `1` | ❌ `0` |
| **.gg Threshold** | N/A | N/A | `0.8` | `0.6` | `0.8` |
| **FFW Enabled** | ✅ `1` | ✅ `1` | ✅ `1` | ❌ `0` | ❌ `0` |
| **FFW Time** | `240` | `120` | `180` | N/A | `240` |

---

## Quick Decision Checklist

### ✅ Enable Auto-Ready When:
- [ ] Match is scheduled (players expected on time)
- [ ] Fast-paced tournament format
- [ ] Minimal warmup needed
- [ ] Practice/scrim environment

### ✅ Limit Pauses When:
- [ ] Official tournament (prevents abuse)
- [ ] Fast-paced format (minimize delays)
- [ ] Competitive/ranked play

### ✅ Enable .gg When:
- [ ] Ranked/matchmaking (allow surrenders)
- [ ] Practice/scrims (quick exits)
- [ ] Unranked tournaments

### ❌ Disable .gg When:
- [ ] Official tournaments (no forfeits allowed)
- [ ] High-stakes matches
- [ ] Any match where every round matters

### ✅ Enable FFW When:
- [ ] Online matches (connection issues expected)
- [ ] Automated platforms (handle abandons)
- [ ] Tournament with walkover rules

### ❌ Disable FFW When:
- [ ] LAN tournaments (no connection issues)
- [ ] Matches requiring manual admin intervention
- [ ] Practice/casual play

---

## Key Principles

1. **Start Conservative**: Use default/safe template when uncertain
2. **Official Matches**: Disable forfeits, limit pauses, enable FFW
3. **Online vs LAN**: Always enable FFW for online, disable for LAN
4. **Fast Formats**: Enable auto-ready, shorter timers
5. **Casual Play**: Enable .gg, unlimited pauses, disable FFW
6. **Backward Compatible**: All new convars are optional

---

## Common Mistakes to Avoid

❌ **DON'T:**
- Enable `.gg` in official tournaments
- Disable FFW for online tournaments
- Set pause limits too low (< 1 per team)
- Set FFW time too short (< 120 seconds)
- Enable auto-ready for first-time players

✅ **DO:**
- Test configurations in scrims first
- Communicate rules to players
- Monitor and adjust based on feedback
- Use default template when uncertain

---

## Backward Compatibility

- ✅ All new convars are **optional**
- ✅ Omitting them uses safe defaults
- ✅ Old match configs work without changes
- ✅ Can be set per-match via JSON `cvars` object
- ✅ Server defaults can be overridden per-match

---

## Complete Default Values Reference

```json
{
  "matchzy_autoready_enabled": 0,
  "matchzy_both_teams_unpause_required": 1,
  "matchzy_max_pauses_per_team": 0,
  "matchzy_pause_duration": 0,
  "matchzy_side_selection_enabled": 1,
  "matchzy_side_selection_time": 60,
  "matchzy_gg_enabled": 0,
  "matchzy_gg_threshold": 0.8,
  "matchzy_ffw_enabled": 0,
  "matchzy_ffw_time": 240
}
```

### Default Behavior Summary

| ConVar | Default | Meaning |
|--------|---------|---------|
| `matchzy_autoready_enabled` | `0` | Players must manually `.ready` |
| `matchzy_both_teams_unpause_required` | `1` | Both teams must `.unpause` |
| `matchzy_max_pauses_per_team` | `0` | Unlimited pauses |
| `matchzy_pause_duration` | `0` | No pause time limit |
| `matchzy_side_selection_enabled` | `1` | Timer enabled after knife |
| `matchzy_side_selection_time` | `60` | 60 seconds to choose side |
| `matchzy_gg_enabled` | `0` | `.gg` command disabled |
| `matchzy_gg_threshold` | `0.8` | 80% team vote (if enabled) |
| `matchzy_ffw_enabled` | `0` | No automatic forfeit |
| `matchzy_ffw_time` | `240` | 4 minutes (if enabled) |

---

## Integration Example (Python)

```python
def generate_match_config(match_type, is_online=True):
    """
    Generate MatchZy Enhanced configuration based on match type.
    
    Args:
        match_type: "official", "fast", "ranked", or "scrim"
        is_online: True for online matches, False for LAN
    
    Returns:
        dict: Match configuration with cvars
    """
    
    # Base configuration (defaults)
    config = {
        "matchid": f"{match_type}_{generate_match_id()}",
        "cvars": {
            "matchzy_autoready_enabled": 0,
            "matchzy_both_teams_unpause_required": 1,
            "matchzy_max_pauses_per_team": 0,
            "matchzy_pause_duration": 0,
            "matchzy_side_selection_enabled": 1,
            "matchzy_side_selection_time": 60,
            "matchzy_gg_enabled": 0,
            "matchzy_gg_threshold": 0.8,
            "matchzy_ffw_enabled": 0,
            "matchzy_ffw_time": 240
        }
    }
    
    # Apply match type specific settings
    if match_type == "official":
        config["cvars"].update({
            "matchzy_autoready_enabled": 0,
            "matchzy_max_pauses_per_team": 2,
            "matchzy_pause_duration": 300,
            "matchzy_side_selection_time": 60,
            "matchzy_gg_enabled": 0,
            "matchzy_ffw_enabled": 1 if is_online else 0,
            "matchzy_ffw_time": 240
        })
    
    elif match_type == "fast":
        config["cvars"].update({
            "matchzy_autoready_enabled": 1,
            "matchzy_max_pauses_per_team": 1,
            "matchzy_pause_duration": 180,
            "matchzy_side_selection_time": 30,
            "matchzy_gg_enabled": 0,
            "matchzy_ffw_enabled": 1 if is_online else 0,
            "matchzy_ffw_time": 120
        })
    
    elif match_type == "ranked":
        config["cvars"].update({
            "matchzy_autoready_enabled": 0,
            "matchzy_max_pauses_per_team": 2,
            "matchzy_pause_duration": 300,
            "matchzy_side_selection_time": 45,
            "matchzy_gg_enabled": 1,
            "matchzy_gg_threshold": 0.8,
            "matchzy_ffw_enabled": 1 if is_online else 0,
            "matchzy_ffw_time": 180
        })
    
    elif match_type == "scrim":
        config["cvars"].update({
            "matchzy_autoready_enabled": 1,
            "matchzy_both_teams_unpause_required": 0,
            "matchzy_max_pauses_per_team": 0,
            "matchzy_pause_duration": 0,
            "matchzy_gg_enabled": 1,
            "matchzy_gg_threshold": 0.6,
            "matchzy_ffw_enabled": 0
        })
    
    return config

# Usage examples
official_match = generate_match_config("official", is_online=True)
fast_match = generate_match_config("fast", is_online=True)
scrim_match = generate_match_config("scrim", is_online=False)
```

---

## LLM System Prompt Suggestion

```
You are configuring CS2 matches using MatchZy Enhanced v1.3.0. When creating match configurations:

1. ALWAYS start with defaults (safe values)
2. For official tournaments: Disable .gg, limit pauses (2 per team, 300s), enable FFW if online
3. For fast tournaments: Enable auto-ready, reduce timers (30s side selection, 1 pause, 180s limit)
4. For ranked: Enable .gg (0.8 threshold), limit pauses, enable FFW
5. For scrims: Enable auto-ready and .gg (0.6 threshold), unlimited pauses
6. NEVER enable .gg in official tournaments
7. ALWAYS enable FFW for online matches (except scrims)
8. NEVER enable FFW for LAN matches

Default values:
- matchzy_autoready_enabled: 0 (manual ready)
- matchzy_max_pauses_per_team: 0 (unlimited)
- matchzy_pause_duration: 0 (unlimited)
- matchzy_gg_enabled: 0 (disabled)
- matchzy_ffw_enabled: 0 (disabled)

When uncertain, use Template 5 (Default/Safe Configuration).
```

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────────┐
│ MatchZy Enhanced v1.3.0 - Quick Reference                   │
├─────────────────────────────────────────────────────────────┤
│ AUTO-READY (default: 0)                                     │
│  0 = Manual ready  |  1 = Auto-ready on join               │
├─────────────────────────────────────────────────────────────┤
│ PAUSES (defaults: 1, 0, 0)                                  │
│  both_teams: 0=single 1=both                                │
│  max_per_team: 0=unlimited, 1-999=limit                     │
│  duration: 0=unlimited, seconds=timeout                     │
├─────────────────────────────────────────────────────────────┤
│ SIDE SELECTION (defaults: 1, 60)                            │
│  enabled: 0=no timer  1=timer                               │
│  time: seconds before random pick                           │
├─────────────────────────────────────────────────────────────┤
│ .GG FORFEIT (defaults: 0, 0.8)                              │
│  enabled: 0=disabled  1=enabled                             │
│  threshold: 0.0-1.0 (% of team)                             │
├─────────────────────────────────────────────────────────────┤
│ FFW WALKOVER (defaults: 0, 240)                             │
│  enabled: 0=disabled  1=enabled                             │
│  time: seconds before forfeit                               │
└─────────────────────────────────────────────────────────────┘

TEMPLATES:
  Official: Strict rules, no .gg, 2 pauses, FFW enabled
  Fast: Auto-ready, 1 pause, short timers, FFW enabled
  Ranked: Allow .gg, 2 pauses, FFW enabled
  Scrim: Relaxed, .gg easy, unlimited pauses, no FFW
  Default: All defaults (safest)
```

---

## Version History

- **v1.3.0** (2026-01-19): Initial release of enhanced features
  - Added 10 new configuration variables
  - All features disabled/unlimited by default for safety
  - Full backward compatibility maintained

---

**Last Updated:** January 19, 2026  
**Plugin Version:** MatchZy Enhanced v1.3.0  
**For Questions:** [GitHub Issues](https://github.com/sivert-io/MatchZy-Enhanced/issues)
