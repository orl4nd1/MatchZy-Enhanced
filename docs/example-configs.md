# Example Configuration Snippets

This page provides ready-to-use configuration templates for common use cases with MatchZy Enhanced.

## Quick Setup

All examples assume you're editing `cfg/MatchZy/config.cfg`. Add or modify the relevant lines for your use case.

---

## 🏆 Tournament Configurations

### Fast-Paced Online Tournament

Optimized for quick turnaround, strict timing, and minimal downtime.

```cfg
// Auto-ready to speed up match starts
matchzy_autoready_enabled "1"

// Strict pause controls
matchzy_both_teams_unpause_required "1"
matchzy_max_pauses_per_team "2"        // 2 pauses per team maximum
matchzy_pause_duration "300"           // 5-minute pause limit

// Quick side selection
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "45"       // 45 seconds to choose

// Disable .gg (prevent premature forfeits)
matchzy_gg_enabled "0"

// Enable FFW with short timer
matchzy_ffw_enabled "1"
matchzy_ffw_time "180"                 // 3-minute forfeit timer
```

### Professional League Match

Balanced configuration for high-stakes competitive play.

```cfg
// Manual ready (players confirm readiness)
matchzy_autoready_enabled "0"

// Both teams must agree to unpause
matchzy_both_teams_unpause_required "1"
matchzy_max_pauses_per_team "3"        // 3 pauses per team
matchzy_pause_duration "0"             // No time limit

// Extended side selection time
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "90"       // 90 seconds for discussion

// Disable .gg (no early forfeits in league)
matchzy_gg_enabled "0"

// Enable FFW with longer timer
matchzy_ffw_enabled "1"
matchzy_ffw_time "300"                 // 5-minute forfeit timer
```

### Major Tournament / LAN Event

Maximum control and flexibility for event administrators.

```cfg
// Manual ready (tournament admin controls)
matchzy_autoready_enabled "0"

// Both teams coordinate pauses
matchzy_both_teams_unpause_required "1"
matchzy_max_pauses_per_team "0"        // Unlimited (admin supervised)
matchzy_pause_duration "0"             // No limit (admin supervised)

// Extended side selection
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "120"      // 2 minutes for team discussion

// Disable .gg (admin controls match flow)
matchzy_gg_enabled "0"

// Disable FFW (LAN environment, no disconnects expected)
matchzy_ffw_enabled "0"
```

---

## 🎮 Practice & Scrim Configurations

### Casual Scrims

Relaxed settings for practice matches between teams.

```cfg
// Auto-ready for quick starts
matchzy_autoready_enabled "1"

// Relaxed pause rules
matchzy_both_teams_unpause_required "0"  // Any team can unpause
matchzy_max_pauses_per_team "0"          // Unlimited pauses
matchzy_pause_duration "0"               // No time limit

// Standard side selection
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "60"

// Enable .gg for quick forfeits
matchzy_gg_enabled "1"
matchzy_gg_threshold "0.8"               // 80% team consensus

// Disable FFW (scrims are informal)
matchzy_ffw_enabled "0"
```

### Team Practice / Inhouse

Settings for same-org practice or inhouse matches.

```cfg
// Auto-ready (everyone's on Discord anyway)
matchzy_autoready_enabled "1"

// Single-team unpause
matchzy_both_teams_unpause_required "0"
matchzy_max_pauses_per_team "0"          // Unlimited
matchzy_pause_duration "0"               // No limit

// Quick side selection
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "30"         // 30 seconds

// Enable .gg with lower threshold
matchzy_gg_enabled "1"
matchzy_gg_threshold "0.6"               // 60% (3/5 players)

// Disable FFW
matchzy_ffw_enabled "0"
```

---

## 🎯 Ranked / Ladder Configurations

### Ranked Matchmaking

For automated ranked/ladder systems with anti-abuse measures.

```cfg
// Manual ready (prevents AFK players)
matchzy_autoready_enabled "0"

// Both teams must unpause
matchzy_both_teams_unpause_required "1"
matchzy_max_pauses_per_team "1"          // 1 pause per team
matchzy_pause_duration "180"             // 3-minute maximum

// Standard side selection
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "60"

// Disable .gg (prevent ELO abuse)
matchzy_gg_enabled "0"

// Enable FFW with standard timer
matchzy_ffw_enabled "1"
matchzy_ffw_time "240"                   // 4 minutes
```

### Community Pug System

For community-run pickup game systems.

```cfg
// Auto-ready (players join intentionally)
matchzy_autoready_enabled "1"

// Both teams unpause
matchzy_both_teams_unpause_required "1"
matchzy_max_pauses_per_team "2"
matchzy_pause_duration "240"             // 4 minutes

// Quick side selection
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "45"

// Enable .gg with high threshold
matchzy_gg_enabled "1"
matchzy_gg_threshold "1.0"               // 100% (all 5 players)

// Enable FFW
matchzy_ffw_enabled "1"
matchzy_ffw_time "240"
```

---

## 🔧 Testing & Development

### Testing/QA Configuration

For testing new features and validating functionality.

```cfg
// Auto-ready for quick iterations
matchzy_autoready_enabled "1"

// Minimal restrictions
matchzy_both_teams_unpause_required "0"
matchzy_max_pauses_per_team "0"
matchzy_pause_duration "0"

// Short timers for testing
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "15"         // 15 seconds for quick tests

// Enable .gg for quick test resets
matchzy_gg_enabled "1"
matchzy_gg_threshold "0.5"               // 50% for easy testing

// Enable FFW with short timer
matchzy_ffw_enabled "1"
matchzy_ffw_time "60"                    // 1 minute for testing
```

---

## 📊 Feature-Specific Examples

### Disable All Enhanced Features

Return to classic MatchZy behavior (all new features off).

```cfg
matchzy_autoready_enabled "0"
matchzy_both_teams_unpause_required "1"  // Keep this, it's good
matchzy_max_pauses_per_team "0"
matchzy_pause_duration "0"
matchzy_side_selection_enabled "1"       // Keep timer, it's helpful
matchzy_gg_enabled "0"
matchzy_ffw_enabled "0"
```

### Maximum Strictness (Anti-Abuse)

Strictest possible settings to prevent abuse.

```cfg
matchzy_autoready_enabled "0"            // Manual ready only
matchzy_both_teams_unpause_required "1"  // Both teams must agree
matchzy_max_pauses_per_team "1"          // Only 1 pause
matchzy_pause_duration "120"             // 2-minute limit
matchzy_side_selection_enabled "1"
matchzy_side_selection_time "30"         // 30 seconds only
matchzy_gg_enabled "0"                   // No forfeit option
matchzy_ffw_enabled "1"
matchzy_ffw_time "180"                   // 3-minute disconnect timer
```

### Maximum Freedom (Practice Server)

Most relaxed settings for practice environments.

```cfg
matchzy_autoready_enabled "1"
matchzy_both_teams_unpause_required "0"
matchzy_max_pauses_per_team "0"          // Unlimited
matchzy_pause_duration "0"               // No limit
matchzy_side_selection_enabled "0"       // Disable timer
matchzy_gg_enabled "1"
matchzy_gg_threshold "0.5"               // Easy forfeit
matchzy_ffw_enabled "0"                  // No auto-forfeit
```

---

## 🔍 Configuration Tips

### Finding the Right Balance

1. **Start Conservative**: Begin with stricter settings and relax them based on feedback
2. **Monitor Abuse**: Watch for pause spam, early forfeits, etc.
3. **Adjust Gradually**: Make small increments (e.g., 2→3 pauses, not 2→unlimited)
4. **Player Education**: Ensure players understand the rules before enabling features

### Common Mistakes to Avoid

❌ **Don't enable auto-ready for ranked** - AFK players will ruin matches
❌ **Don't set pause limits too low** - Legitimate technical issues need time
❌ **Don't disable FFW for online** - Connection issues need fair handling
❌ **Don't enable .gg in competitive** - Use only for casual/practice

### Testing Your Configuration

1. Load your config on a test server
2. Try each new feature with 2-3 players
3. Test edge cases (timeout expiry, threshold votes, etc.)
4. Verify behavior matches expectations
5. Deploy to production servers

---

## 📚 Additional Resources

- **[Configuration Reference](configuration.md)** - Detailed convar documentation
- **[Commands Reference](commands.md)** - All available commands
- **[CHANGELOG](CHANGELOG.md)** - What's new in v1.3.0

---

<div align="center">

**Questions?** Check the [MatchZy Enhanced Documentation](https://mat.sivert.io/)

</div>
