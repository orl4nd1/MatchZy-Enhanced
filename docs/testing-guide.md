# Testing Guide - Enhanced Features v1.3.0

This document provides comprehensive test scenarios for validating all new features in MatchZy Enhanced v1.3.0.

## Test Environment Setup

### Prerequisites
- CS2 Dedicated Server with MatchZy Enhanced v1.3.0+
- Minimum 2 test players (4-6 recommended for full testing)
- Admin access to server console/RCON
- Test match configuration JSON

### Basic Test Setup
1. Start server with default config
2. Load a test match: `matchzy_loadmatch_url <test_match_url>`
3. Have players join appropriate teams
4. Verify base functionality before testing enhanced features

---

## Feature Test Scenarios

### 1. Auto-Ready System

**Config:**
```cfg
matchzy_autoready_enabled "1"
```

#### Test 1.1: Auto-Ready on Join
**Steps:**
1. Enable auto-ready in config
2. Start warmup
3. Player joins server
4. Check player status

**Expected:**
- Player automatically marked as ready
- Chat message: "You have been automatically marked as ready. Type .unready if you are not ready."
- `player_ready` event sent
- Match starts when minimum players reached

**Validation:**
- ✅ Player shows as ready in status
- ✅ Match starts automatically with minimum players
- ✅ Event logged in console

#### Test 1.2: Unready with Auto-Ready Enabled
**Steps:**
1. Join with auto-ready enabled
2. Type `.unready`
3. Check status

**Expected:**
- Player marked as not ready
- Chat message: "You have been marked as NOT ready."
- `player_unready` event sent
- Match doesn't start until player readies again

**Validation:**
- ✅ Can opt-out of auto-ready
- ✅ Match respects unready status

#### Test 1.3: Auto-Ready Disabled
**Steps:**
1. Set `matchzy_autoready_enabled "0"`
2. Reload match
3. Player joins

**Expected:**
- Player NOT automatically ready
- Must manually type `.ready`
- Classic behavior

---

### 2. Enhanced Pause System

**Config:**
```cfg
matchzy_both_teams_unpause_required "1"
matchzy_max_pauses_per_team "2"
matchzy_pause_duration "300"
```

#### Test 2.1: Both Teams Unpause Required
**Steps:**
1. Start live match
2. Team A player types `.pause`
3. Team A player types `.unpause`
4. Check pause status
5. Team B player types `.unpause`

**Expected:**
- Match pauses after Team A `.pause`
- Team A `.unpause`: Message "Team A wants to unpause. Team B, please write !unpause to confirm."
- Match still paused
- Team B `.unpause`: Match unpauses
- Chat: "Both teams have unpaused, resuming the match!"

**Validation:**
- ✅ Both teams must agree
- ✅ Proper messaging
- ✅ Match resumes only after both

#### Test 2.2: Pause Limit Enforcement
**Steps:**
1. Set `matchzy_max_pauses_per_team "2"`
2. Team A pauses (pause 1)
3. Unpause
4. Team A pauses (pause 2)
5. Unpause
6. Team A tries to pause (pause 3)

**Expected:**
- First 2 pauses succeed with messages showing remaining count
- Third pause blocked
- Message: "Team A has no more pauses left (2 max)."

**Validation:**
- ✅ Limit enforced correctly
- ✅ Counter accurate
- ✅ No bypass available

#### Test 2.3: Pause Timeout
**Steps:**
1. Set `matchzy_pause_duration "60"` (1 minute for testing)
2. Team A pauses
3. Wait 60 seconds
4. Check match status

**Expected:**
- After 60 seconds: "Pause timeout expired, resuming match."
- Match automatically unpauses
- Round resumes

**Validation:**
- ✅ Timer works correctly
- ✅ Auto-unpause triggers
- ✅ Match continues normally

#### Test 2.4: Command Aliases
**Steps:**
1. Type `.p` to pause
2. Type `.up` to unpause

**Expected:**
- `.p` works identically to `.pause`
- `.up` works identically to `.unpause`
- Same behavior as full commands

**Validation:**
- ✅ Aliases function correctly

#### Test 2.5: Single-Team Unpause Mode
**Steps:**
1. Set `matchzy_both_teams_unpause_required "0"`
2. Team A pauses
3. Team B types `.unpause`

**Expected:**
- Match unpauses immediately
- Message: "Team B has unpaused the match!"
- No confirmation needed

**Validation:**
- ✅ Any team can unpause
- ✅ Immediate effect

---

### 3. Side Selection Timer

**Config:**
```cfg
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "60"
```

#### Test 3.1: Timer Display and Selection
**Steps:**
1. Play knife round
2. Team wins knife
3. Observe timer message
4. Winner types `.stay` before timer expires

**Expected:**
- After knife: "Team A won the knife round. They have 60 seconds to type .stay or .switch."
- Winner team types `.stay` or `.switch`
- Timer cancels
- Match goes live

**Validation:**
- ✅ Timer message shows
- ✅ Commands work during timer
- ✅ Timer cancels on choice

#### Test 3.2: Timer Expiry - Random Selection
**Steps:**
1. Play knife round
2. Team wins knife
3. Wait full 60 seconds without choosing

**Expected:**
- At 60 seconds: Random side chosen
- Message: "Team A did not choose a side. Random selection: staying/swapping."
- Match goes live with random choice

**Validation:**
- ✅ Random selection triggers
- ✅ Message accurate
- ✅ Match proceeds

#### Test 3.3: Alternative Commands (.ct/.t)
**Steps:**
1. Win knife as T
2. Type `.ct`

**Expected:**
- Sides swap (equivalent to `.switch`)
- Match goes live

**Steps:**
1. Win knife as CT
2. Type `.ct`

**Expected:**
- Sides stay (equivalent to `.stay`)
- Match goes live

**Validation:**
- ✅ `.ct`/`.t` work correctly
- ✅ Logic based on current side

#### Test 3.4: Timer Disabled
**Steps:**
1. Set `matchzy_side_selection_enabled "0"`
2. Play knife round
3. Wait indefinitely

**Expected:**
- No timer message
- Can choose side at any time
- Classic behavior

**Validation:**
- ✅ Timer optional

---

### 4. Early Match Termination (.gg)

**Config:**
```cfg
matchzy_gg_enabled "1"
matchzy_gg_threshold "0.8"
```

#### Test 4.1: Vote Counting (5v5)
**Steps:**
1. Start 5v5 match
2. Team A player 1 types `.gg`
3. Team A player 2 types `.gg`
4. Team A player 3 types `.gg`
5. Check match status
6. Team A player 4 types `.gg`

**Expected:**
- After each vote: "PlayerX from Team A voted to end the match (X/4 votes)."
- After 4th vote (80% of 5 = 4): "Team A has reached the threshold to end the match. The opposing team wins!"
- Match ends
- Team B wins 16-0

**Validation:**
- ✅ Vote counting accurate
- ✅ Threshold calculation correct (80% of 5 = 4)
- ✅ Opposing team wins
- ✅ Match ends immediately

#### Test 4.2: Duplicate Vote Prevention
**Steps:**
1. Player types `.gg`
2. Same player types `.gg` again

**Expected:**
- First vote counts
- Second vote rejected
- Message: "You have already voted to end the match."

**Validation:**
- ✅ No double voting

#### Test 4.3: Vote Reset Per Round
**Steps:**
1. Round 1: Team A gets 2 votes for `.gg`
2. Round ends
3. Round 2: Check vote count

**Expected:**
- Votes reset to 0 at round start
- Players must vote again in new round

**Validation:**
- ✅ Votes don't carry over rounds

#### Test 4.4: Spectator/Unassigned Players
**Steps:**
1. Join as spectator
2. Type `.gg`

**Expected:**
- Message: "You must be on a team (CT or T) to use .gg."
- No vote counted

**Validation:**
- ✅ Only team players can vote

#### Test 4.5: .gg Disabled
**Steps:**
1. Set `matchzy_gg_enabled "0"`
2. Type `.gg`

**Expected:**
- Message: "The .gg command is disabled on this server."
- No forfeit

**Validation:**
- ✅ Can be disabled

#### Test 4.6: Different Thresholds
**Test 100% threshold (5/5):**
```cfg
matchzy_gg_threshold "1.0"
```
- Requires all 5 players to vote

**Test 60% threshold (3/5):**
```cfg
matchzy_gg_threshold "0.6"
```
- Requires only 3 players to vote

**Validation:**
- ✅ Threshold adjustable
- ✅ Math correct

---

### 5. FFW (Forfeit/Walkover) System

**Config:**
```cfg
matchzy_ffw_enabled "1"
matchzy_ffw_time "240"
```

#### Test 5.1: FFW Timer Start
**Steps:**
1. Start live 5v5 match
2. All 5 players from Team A disconnect
3. Observe server

**Expected:**
- Immediately after last disconnect: "Team A has left the server. Forfeit timer started: 4 minute(s) remaining."
- FFW timer begins

**Validation:**
- ✅ Timer starts on full team disconnect
- ✅ Message accurate

#### Test 5.2: FFW Warnings
**Steps:**
1. Trigger FFW as above
2. Wait intervals

**Expected:**
- After 1 minute: "Warning: Team A still missing. Forfeit in 3 minute(s) if no one rejoins."
- After 2 minutes: "Warning: Team A still missing. Forfeit in 2 minute(s) if no one rejoins."
- After 3 minutes: "Warning: Team A still missing. Forfeit in 1 minute(s) if no one rejoins."

**Validation:**
- ✅ Warnings every minute
- ✅ Countdown accurate

#### Test 5.3: FFW Execution
**Steps:**
1. Trigger FFW
2. Wait full 4 minutes

**Expected:**
- After 4 minutes: "Team A forfeited the match. Team B wins by forfeit!"
- Team B score set to 16
- Team A score set to 0
- Match ends

**Validation:**
- ✅ Forfeit executes correctly
- ✅ Scores set properly
- ✅ Match ends

#### Test 5.4: FFW Cancellation
**Steps:**
1. Trigger FFW (all Team A disconnects)
2. Wait 2 minutes
3. Any Team A player reconnects

**Expected:**
- On reconnect: "Team A returned! Forfeit timer cancelled."
- Timer stops
- Match continues normally

**Validation:**
- ✅ Cancels on player return
- ✅ Match resumes normally

#### Test 5.5: Partial Team Disconnect
**Steps:**
1. In 5v5, have 4 Team A players disconnect
2. 1 Team A player remains connected

**Expected:**
- FFW does NOT start
- Match continues (though heavily disadvantaged)

**Validation:**
- ✅ Only triggers on FULL team disconnect

#### Test 5.6: FFW Disabled
**Steps:**
1. Set `matchzy_ffw_enabled "0"`
2. All Team A players disconnect

**Expected:**
- No FFW timer
- No forfeit
- Match just sits in disadvantaged state

**Validation:**
- ✅ FFW can be disabled

---

## Integration Testing

### Integration Test 1: Auto-Ready + Match Start
1. Enable auto-ready
2. Have exactly minimum players join
3. Verify match starts automatically

**Expected:** Seamless auto-start

### Integration Test 2: Pause Limit + Tactical Timeout
1. Set pause limit to 1
2. Use 1 regular pause
3. Try regular pause again (blocked)
4. Use tactical timeout

**Expected:** Tactical timeout still works independently

### Integration Test 3: Side Selection Timer + Knife Round
1. Play full knife round
2. Timer starts immediately after knife
3. Choose side before expiry

**Expected:** Smooth transition

### Integration Test 4: FFW + Pause System
1. Pause match
2. All one team disconnects
3. Observe behavior

**Expected:** FFW should handle properly (may want to cancel timer during pause)

### Integration Test 5: .gg + Overtime
1. Reach overtime (15-15)
2. Team votes to .gg

**Expected:** Forfeit works in OT

### Integration Test 6: Multiple Features Enabled
1. Enable all features
2. Play full match
3. Use each feature at different times

**Expected:** No conflicts or issues

---

## Edge Cases & Stress Testing

### Edge Case 1: Map Change During Timer
- Trigger side selection timer
- Admin forces map change
- Expected: Timers cleaned up properly

### Edge Case 2: Match Reset During FFW
- FFW timer active
- Admin types `.restart`
- Expected: FFW timer cancelled, match resets

### Edge Case 3: Rapid Pause/Unpause
- Pause and unpause repeatedly
- Expected: No crashes, limits enforced

### Edge Case 4: .gg with Varying Team Sizes
- Test .gg in 2v2 (80% = 2 players)
- Test .gg in 3v3 (80% = 3 players)
- Test .gg in 4v4 (80% = 4 players)
- Expected: Math correct for all sizes

### Edge Case 5: Player Disconnect During Vote
- 4/5 players vote .gg
- 5th player disconnects before voting
- Expected: Threshold should still calculate based on connected players

---

## Regression Testing

Verify existing features still work:

1. **Basic Ready System** - Manual `.ready` / `.unready` still works
2. **Knife Round** - Knife round plays normally
3. **Pause/Unpause** - Basic pause still works
4. **Tactical Timeout** - `.tac` still works
5. **Match Flow** - Warmup → Live → Halftime → Live → End
6. **Score Tracking** - Scores update correctly
7. **Demo Recording** - Demos still record
8. **Events** - All events still fire
9. **Admin Commands** - Admin overrides still work
10. **Practice Mode** - Practice mode unaffected

---

## Automated Test Checklist

- [ ] All 6 convars set correctly in config
- [ ] All convars readable via console
- [ ] All 3 new commands (`.gg`, `.p`, `.up`) work
- [ ] Enhanced commands (`.ct`, `.t`) work
- [ ] All localiz strings display correctly
- [ ] No console errors during operation
- [ ] No memory leaks over time
- [ ] Timers clean up on match reset
- [ ] Events fire correctly
- [ ] Compatible with existing features

---

## Bug Reporting Template

If you find issues during testing:

```markdown
**Feature:** [Auto-Ready / Pause / Side Selection / .gg / FFW]

**Configuration:**
```cfg
[paste relevant convars]
```

**Steps to Reproduce:**
1. 
2. 
3. 

**Expected Behavior:**


**Actual Behavior:**


**Console Output:**
```
[paste relevant console logs]
```

**Server Info:**
- MatchZy Enhanced Version: 
- CS2 Build: 
- CounterStrikeSharp Version: 
- Players Online: 

**Additional Context:**
```

---

## Test Sign-Off

Once all tests pass, document results:

- [ ] Auto-Ready System: ✅ PASS / ❌ FAIL
- [ ] Enhanced Pause System: ✅ PASS / ❌ FAIL
- [ ] Side Selection Timer: ✅ PASS / ❌ FAIL
- [ ] .gg Command: ✅ PASS / ❌ FAIL
- [ ] FFW System: ✅ PASS / ❌ FAIL
- [ ] Integration Tests: ✅ PASS / ❌ FAIL
- [ ] Regression Tests: ✅ PASS / ❌ FAIL

**Tester:** _______________
**Date:** _______________
**Version Tested:** _______________

---

<div align="center">

**Ready for production?** ✅

All tests must pass before deploying to live servers.

</div>
