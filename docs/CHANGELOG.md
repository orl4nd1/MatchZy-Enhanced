# MatchZy Enhanced Changelog

> **Note:** Versions 1.0.0+ are MatchZy Enhanced - a fork optimized for tournament automation.
> Original MatchZy versions (0.8.x and earlier) are included below for reference.

---

# 1.4.9

#### January 24, 2026

### ✨ New Features

#### Enhanced Notification System with Duration Support
- **Timed notifications**: Messages can now be displayed for a specified duration, enhancing visibility during matches
- **Unique notification keys**: Multiple notifications can be managed effectively without overlap
- **Duration parameters**: All notification methods now support duration parameters, ensuring messages remain visible for the specified time
- Improved notification management for better user experience during critical match moments

### 🔧 Technical Changes

- Updated notification system architecture to support timed display
- Implemented unique notification key management to prevent notification conflicts
- Enhanced existing notification methods with duration support

---

# 1.4.8

#### January 24, 2026

### ✨ New Features

#### Center HTML Notifications (Experimental)
- **Global notifications**: Match live, pause countdown, side selection displayed to all players
- **Personal notifications**: Individual ready/unready status shown to specific players
- **Team-specific notifications**: Unpause confirmation requests shown only to relevant team
- **Countdown timers**: Live countdowns for pause duration, side selection, and server restart
- Toggle with `matchzy_center_html_notifications` (default: **disabled** - opt-in feature)
- Clean, professional notifications that don't clutter chat

### 🐛 Bug Fixes

- **Fixed**: `server_configured` event now requires `matchzy_server_id` to be set before sending
  - Prevents events with empty server_id from being sent
  - Auto-triggers event when server_id is set after URL configuration
- **Fixed**: Event retry queue threading issues
  - Database operations now run on main game thread
  - Prevents "Invoked on a non-main thread" errors
- **Fixed**: Event retry queue now automatically clears when webhook URL changes
  - Prevents retrying events to old/wrong URLs
  - New URL configuration immediately takes effect
  - Added `matchzy_clear_event_queue` command to manually clear pending events

---

# 1.4.0

#### January 20, 2026

**🚀 Bulletproof Event Delivery System**

This release introduces a comprehensive **event reliability and retry system** that ensures zero data loss, even during API downtime, network issues, or server crashes.

### ✨ New Features

#### Event Queue & Automatic Retry
- **Automatic queueing**: Failed webhook events are saved to local database
- **Background retry**: Processes retry queue every 30 seconds with exponential backoff (30s → 1m → 2m → 4m → 8m → 16m → 32m max)
- **Maximum 20 retries** before marking event as `failed`
- **Auto-cleanup**: Successfully sent events removed after 7 days
- **Database table**: `matchzy_event_queue` stores event type, data, match ID, retry count, timestamps, and status

#### Server Registration Event
- **`server_configured` event**: Automatically sent when webhook URL is set or on server startup
- Contains server ID, hostname, plugin version, webhook URL, and timestamp
- Enables API to track active servers and maintain server status database
- Useful for dashboards, health monitoring, and server management

#### Pull API for Data Recovery
- **`matchzy_get_match_stats <matchId>`**: Returns complete match statistics as JSON from local database
- **`matchzy_get_pending_events`** (admin only): Shows event queue status with breakdown by event type
- Enables tournament platforms to pull missing data directly from CS2 servers

#### Smart Console Logging
- Visual feedback when events fail and are queued for retry
- Success messages when retried events are delivered
- Example: `[MatchZy Events] ✗ FAILED to send 'round_end' (HTTP 500) → Event queued for retry`
- Example: `[MatchZy Events] ✓ Retry successful: 'round_end' (attempt 3)`

### 🔧 Technical Details

**Database schema:**
- New `matchzy_event_queue` table for both SQLite and MySQL
- Tracks event type, full JSON payload, match ID, map number, retry count, next retry time, status, and error messages

**Event flow:**
1. Event POST fails (non-2xx, timeout, or exception) → Auto-queued
2. Background timer (30s interval) processes pending events
3. Exponential backoff prevents API overload
4. Successful delivery → Mark as `sent` and remove after 7 days
5. Max retries reached → Mark as `failed` for manual review

**Benefits:**
- ✅ Zero data loss during API downtime
- ✅ Works on managed hosting (Dathost, etc.) - no database exposure needed
- ✅ Each server manages its own queue independently
- ✅ No manual intervention required
- ✅ Pull API available for data recovery

### 📚 Documentation

- Updated `configuration.md` with event reliability section
- Updated `commands.md` with new commands and permission reference
- Updated `integration.md` with event reliability details

---

# 1.4.2

#### January 20, 2026

- **Localized match winner messages**: Added `matchzy.match.won` localization key to all 12 supported languages (English, German, Spanish, French, Hungarian, Japanese, Portuguese BR/PT, Russian, Uzbek, Chinese Simplified/Traditional)
- **Enhanced kick messages**: Players now see the match winner message 3 times in different colors (Lime → Green → Yellow) with a 5-second delay before being kicked, ensuring they can read the result even if the disconnect popup doesn't show the reason
- **Improved side selection timer logic**: Enhanced chat reminders and timer handling for better player awareness

# 1.4.1

#### January 20, 2026

- **Refactored event retry timer logic**: Fixed potential timer duplication issues by using a repeating timer pattern with proper flags to prevent multiple instances
- **Improved cleanup timer**: Enhanced old event cleanup logic for better reliability
- **Fixed changelog extraction**: Improved Discord webhook changelog generation to correctly handle release commits and non-existent tags

---

# 1.3.5

#### January 20, 2026

- Introduced **`matchzy_debug_console`** (default: `1`) to control verbose server‑console logging and hooked it into the internal `Log(...)` helper.
- Expanded console‑side debug logs around the **ready/auto‑ready flow**, **match start decisions**, **pause/tactical timeout handling**, **`.gg` votes** and **FFW timers**, to make user‑submitted logs much more useful for debugging.
- Documented both `matchzy_debug_console` and `matchzy_debug_chat` in `configuration.md` / `commands.md` under a dedicated **Debugging & logging** section.

# 1.3.4

#### January 20, 2026

- Added an example configuration for **plugin‑managed pauses** vs native tactical timeouts in `configuration.md` (showing how to use `matchzy_use_pause_command_for_tactical_pause 0` together with `matchzy_max_pauses_per_team`, and how it should align with `mp_team_timeout_max` when using `.tac`).
- Documented the new **`matchzy_gg_min_score_diff`** convar and clarified `.gg` behavior in both `commands.md` and `configuration.md`.
- Added a **reload config** command (`.reload_config` / `matchzy_reload_config`) that safely re‑executes `cfg/MatchZy/config.cfg` when no match is live.
- Restructured `commands.md` to include **linkable per‑command sections** (e.g. `#gg`, `#pause--p`, `#reload_config`) so individual commands can be shared with direct URLs.

# 1.3.3

#### January 20, 2026

- Hardened the **release script** with clean‑tree pre‑checks and safer version handling before tagging/publishing.
- Refactored internal score / readiness logic for clarity and maintainability in the enhanced match‑management features.
- Updated commands and configuration documentation, `README.md`, and `mkdocs.yml` to better reflect the 1.3.x feature set and to streamline the published docs.

# 1.3.0

#### January 19, 2026

**🎉 Major Feature Release: Player Experience Enhancements**

This release introduces a comprehensive suite of new features designed to improve player experience, tournament management, and match flow control. All new features are configurable and disabled by default for safety.

### ✨ New Features

#### Auto-Ready System
- Players are automatically marked as ready when they join the match (configurable)
- Players can still use `.unready` to opt-out if they're not ready
- Match starts automatically when all required players are ready
- Configurable via `matchzy_autoready_enabled` (default: `false`)
- Perfect for fast-paced tournaments where players are expected to be ready

#### Enhanced Pause System
- **Both-team unpause requirement**: Both teams must now type `.unpause` to resume (configurable via `matchzy_both_teams_unpause_required`)
- **Per-team pause limits**: Limit number of pauses per team (e.g., 2 pauses per team) via `matchzy_max_pauses_per_team`
- **Pause duration limits**: Set maximum pause duration with automatic timeout via `matchzy_pause_duration`
- New command aliases: `.p` for `.pause`, `.up` for `.unpause`
- Pause tracking and remaining pauses shown in chat

#### Side Selection Timer
- Configurable timer for side selection after knife round (default: 60 seconds)
- Commands: `.ct`, `.t`, `.stay`, `.swap` all work with timer
- Random side selection if timer expires without player choice
- Configurable via `matchzy_side_selection_enabled` and `matchzy_side_selection_time`
- Prevents indefinite waiting after knife rounds

#### Early Match Termination (`.gg` Command)
- New `.gg` command allows teams to forfeit the match
- Requires 80% team consensus (configurable)
- Vote tracking per round (votes reset each round)
- Opposing team automatically wins when threshold reached
- Configurable via `matchzy_gg_enabled` (default: `false`) and `matchzy_gg_threshold` (default: `0.8`)
- Perfect for scrims and practice matches

#### FFW (Forfeit/Walkover) System
- Automatic forfeit system when entire team leaves the server
- 4-minute timer starts when all players from a team disconnect
- Minute-by-minute warnings in chat
- Automatically cancels if any team member returns
- Opposing team wins by forfeit if timer expires
- Configurable via `matchzy_ffw_enabled` (default: `false`) and `matchzy_ffw_time` (default: `240`)
- Fair handling of connection issues in online tournaments

### 🎮 New Commands

- `.gg` - Vote to end match early (requires team consensus)
- `.up` - Alias for `.unpause`
- `.p` - Alias for `.pause`

### ⚙️ New Configuration Variables

```cfg
// Auto-Ready System
matchzy_autoready_enabled "0"  // Default: disabled

// Enhanced Pause System
matchzy_both_teams_unpause_required "1"  // Default: enabled
matchzy_max_pauses_per_team "0"  // Default: unlimited
matchzy_pause_duration "0"  // Default: no limit

// Side Selection Timer
matchzy_side_selection_enabled "1"  // Default: enabled
matchzy_side_selection_time "60"  // Default: 60 seconds

// Early Match Termination
matchzy_gg_enabled "0"  // Default: disabled
matchzy_gg_threshold "0.8"  // Default: 80%

// FFW System
matchzy_ffw_enabled "0"  // Default: disabled
matchzy_ffw_time "240"  // Default: 4 minutes
```

### 🌍 Localization

- Added English localization for all new features
- New strings ready for community translations in other languages

### 🔧 Technical Changes

- Enhanced event handlers for player connect/disconnect to support new features
- Improved timer management with proper cleanup on match reset
- Thread-safe operations for all new tracking systems
- Comprehensive cleanup of all new timers and tracking in `ResetMatch()`
- **Smart match end delays based on demo configuration:**
  - Demo recording disabled: ~10s restart (was 60-90s)
  - Demo recording enabled, no upload URL: ~25-35s restart (was 60-90s)
  - Demo recording enabled with upload URL: ~60-90s restart (unchanged, waits for upload)

### 📖 Documentation

- Updated `README.md` with new features overview
- Updated `cfg/MatchZy/config.cfg` with comprehensive documentation
- Added detailed comments and use cases for all new convars

---

# 1.2.2

#### January 6, 2026

- Enhanced command documentation and player communication
- Improved practice utilities documentation
- Updated MatchZy convars reference

# 1.2.1

#### December 30, 2025

- Enhanced player whitelist handling to allow admin bypass
- Admins can now connect even if not on whitelist
- Improved match configuration flexibility

# 1.2.0

#### December 30, 2025

**🔄 Remote Backup & Admin Management Release**

This release introduces remote backup upload functionality and per-match admin configuration, enabling better integration with tournament platforms and match management systems.

### ✨ New Features

#### Remote Backup Upload
- Upload match backups to remote HTTP/HTTPS endpoints
- Configurable via `matchzy_remote_backup_url` convar
- Custom authentication headers support via `matchzy_remote_backup_header_key` and `matchzy_remote_backup_header_value`
- Automatic upload after each backup round
- Perfect for centralized backup storage and disaster recovery

#### Per-Match Admin Configuration
- Define admins directly in match JSON configuration
- Override server-level admin settings per match
- Support for `"admins": ["STEAM_ID_1", "STEAM_ID_2"]` in match config
- Simplified tournament admin management
- No need to modify server files for each match

### 🔧 Technical Changes

- Enhanced backup management with HTTP client integration
- Improved admin validation logic
- Better error handling for remote operations

### 📖 Documentation

- Updated configuration guide with remote backup examples
- Added admin configuration documentation

# 1.1.5

#### December 29, 2025

- Enhanced overtime configuration handling
- Improved match management for extended play
- Better overtime round tracking

# 1.1.4

#### December 29, 2025

- Refined overtime handling in match management
- Improved overtime segment tracking
- Better OT phase detection

# 1.1.3

#### December 29, 2025

- Refined overtime segments handling
- Enhanced documentation for overtime configuration
- Improved overtime logic

# 1.1.2

#### December 29, 2025

- Enhanced overtime tie resolution logic
- Improved overtime documentation
- Better handling of OT scenarios

# 1.1.1

#### December 29, 2025

- Added tournament overtime and regulation configuration
- Support for `maxRounds`, `overtimeMode`, and `overtimeSegments` in JSON config
- Enhanced flexibility for tournament organizers

# 1.1.0

#### December 29, 2025

**🎯 Match Queuing & Tournament Automation Release**

This release introduces intelligent match queuing capabilities, allowing servers to automatically transition between matches without manual intervention. Perfect for tournament brackets and continuous match series.

### ✨ New Features

#### Match Queuing System
- New `tournamentNextMatch` field in match JSON configuration
- Automatic detection and loading of queued matches after series end
- Seamless transition between matches without downtime
- Support for tournament bracket automation
- Queue state tracking and validation

#### Overtime Configuration Enhancements
- Support for `maxRounds` in match configuration
- Configurable `overtimeMode` (standard, tournament)
- `overtimeSegments` configuration for multi-OT scenarios
- Better tie resolution logic
- Enhanced overtime round tracking

### 🔧 Technical Changes

- Improved match reset flow to check for queued matches
- Enhanced overtime handling in match management
- Better state management for continuous matches
- Refined OT segment tracking and phase detection

### 📖 Documentation

- Added match queuing examples
- Updated overtime configuration guide
- Enhanced tournament workflow documentation

# 1.0.26

#### December 29, 2025

- Added missing using directive for Utils module
- Fixed compilation issues in G5API.cs

# 1.0.25

#### December 21, 2025

- Implemented connected clients tracking for simulation and normal matches
- Enhanced player state management
- Improved match flow control

# 1.0.24

#### December 20, 2025

- Enhanced simulation and normal match settings enforcement
- Improved timescale and cheats management
- Better simulation mode reliability

# 1.0.23

#### December 20, 2025

- Implemented HLTV/SourceTV exclusion from readiness tracking
- Fixed issues with spectator bots affecting ready system
- Improved match start logic

# 1.0.22

#### December 20, 2025

- Updated simulation initialization logic for first map in series
- Fixed simulation flow for BO3/BO5 matches
- Enhanced multi-map simulation support

# 1.0.21

#### December 20, 2025

- Enhanced simulation mode readiness logic and event handling
- Improved bot ready flow
- Better simulation match lifecycle

# 1.0.20

#### December 19, 2025

- Enhanced logging for series and map checkpoints
- Improved debugging capabilities
- Better match state tracking

# 1.0.19

#### December 19, 2025

- Added simulation timescale configuration
- Support for `simulation_timescale` in match JSON
- Enhanced match control for testing

# 1.0.18

#### December 19, 2025

- Enhanced simulation mode behavior and logging
- Refactored MatchZy configuration
- Improved simulation state management

# 1.0.17

#### December 19, 2025

**🔄 MatchZy-Safe Auto-Updater Release**

Introduces intelligent auto-update detection that prevents server restarts during active matches, critical for unattended tournament servers.

### ✨ New Features

#### MatchZy-Safe Auto-Updater
- Monitors Steam UpToDateCheck API for CS2 server updates
- Only updates when `matchzy_tournament_status` is "idle"
- Writes update markers to disk for external monitoring
- Prevents disruptive mid-match restarts
- Configurable check interval

### 📖 Documentation

- Added auto-updater configuration guide
- Updated tournament server deployment docs

# 1.0.16

#### December 19, 2025

- Enhanced bot behavior in simulation mode
- Improved bot command execution
- Better simulation reliability

# 1.0.15

#### December 19, 2025

- Refactored simulation flow management
- Improved readiness handling for bots
- Enhanced simulation initialization

# 1.0.14

#### December 18, 2025

- Enhanced bot management and simulation flow
- Improved logging and debugging
- Refactored Load method and event handling

# 1.0.13

#### December 18, 2025

- Enhanced simulation ready flow management
- Improved player connection handling
- Better bot management in simulation mode

# 1.0.12

#### December 18, 2025

- Ensured bots remain active in simulation mode
- Fixed `bot_join_after_player` command enforcement
- Supports fully simulated matches without humans

# 1.0.11

#### December 18, 2025

- Refactored bot management and simulation handling
- Improved code organization
- Enhanced simulation reliability

# 1.0.10

#### December 18, 2025

- Refactored bot management logic in Utility.cs
- Cleaned up bot spawn/kick functionality
- Improved code maintainability

# 1.0.9

#### December 18, 2025

- Updated configuration files
- Cleaned up bot management code
- Enhanced simulation configuration

# 1.0.8

#### December 18, 2025

- Refactored simulation handling in MatchZy
- Improved code structure
- Better separation of concerns

# 1.0.7

#### December 18, 2025

- Refactored warmup configuration handling
- Improved simulation warmup logic
- Better configuration management

# 1.0.6

#### December 18, 2025

- Implemented warmup configuration adjustments for simulation mode
- Enhanced simulation warmup flow
- Improved player spawn handling

# 1.0.5

#### December 18, 2025

- Implemented simulation flow handling during map changes
- Fixed simulation initialization on map transitions
- Enhanced multi-map simulation support

# 1.0.4

#### December 18, 2025

- Enhanced remote logging configuration and event handling
- Improved webhook integration
- Better event delivery reliability

# 1.0.3

#### December 10, 2025

- Updated changelog generation in release script
- Improved release automation
- Better GitHub release notes

# 1.0.2

#### December 10, 2025

- Bug fixes and stability improvements
- Enhanced simulation mode handling

# 1.0.1

#### December 10, 2025

- Enhanced simulation mode handling
- Added simulation bot disconnection after series end
- Improved cleanup logic

# 1.0.0

#### December 9, 2025

**🎉 Initial Release: MatchZy Enhanced Fork**

This marks the first official release of the **MatchZy Enhanced** fork, a specialized version optimized for tournament automation, simulation testing, and seamless integration with the MatchZy Auto Tournament platform. Built on the foundation of the excellent original MatchZy by WD-, this fork adds powerful automation capabilities while maintaining full compatibility with the core plugin.

### ✨ New Features

#### Simulation Mode
Complete bot-driven match simulation for testing and automation:
- Configure via `"simulation": true` in match JSON
- Adjustable timescale via `simulation_timescale` for faster testing (1.0 to 10.0x speed)
- Automatic bot spawn and management (10 bots per team)
- Bots remain active even without human players
- Full match lifecycle simulation (warmup, knife, live, overtime, series)
- Complete event logging for automated match processing
- Perfect for tournament bracket pre-computation and stress testing

#### Enhanced Event System
Comprehensive webhook-based event logging for external integrations:
- **Player Events**: Connect, disconnect, ready, unready
- **Match State Events**: Match loaded, started, paused, resumed, ended
- **Round Events**: Round start, end, MVP, bomb plant/defuse
- **Backup Events**: Backup created, loaded
- All events sent to `remote_log_url` as JSON payloads
- Custom authentication header support
- Enables real-time tournament platform integration

#### Tournament Status ConVars
Real-time match state tracking accessible to external tools:
- `matchzy_tournament_status`: Current match phase (idle/loading/warmup/knife/live/paused/halftime/postgame/error)
- `matchzy_tournament_match`: Active match identifier (e.g., "match_12345")
- `matchzy_tournament_updated`: Unix timestamp of last status update
- Perfect for tournament dashboards and server orchestration

#### Match Report API
Automated JSON reports for completed matches:
- Automatic upload to configured endpoint after series end
- Comprehensive match statistics and outcomes
- Server identification for multi-server tournaments
- Authentication header support for secure APIs
- Structured data for database ingestion

#### MatchZy-Safe Auto-Updater
Intelligent update system that never interrupts active matches:
- Monitors Steam UpToDateCheck API for CS2 updates
- Only updates when `matchzy_tournament_status` is "idle"
- Logs update availability to disk for external monitoring
- Prevents mid-match server restarts
- Critical for unattended tournament servers

### 🔧 Technical Changes

- Added `SimulationMode.cs` with complete bot management system
- Enhanced `SynchronizationContextManagement.cs` for thread safety
- Implemented `PublishEvents.cs` for webhook event delivery
- Added `MatchReportCommand.cs` for automated reporting
- Created `MatchZySafeAutoUpdater.cs` with update detection
- Improved state management across all match phases
- Better error handling and logging throughout

### 📖 Documentation

Complete documentation rewrite for tournament automation use cases:
- **API Endpoint Specification**: Full webhook event reference
- **Configuration Loading Behavior**: Priority and override rules
- **Simulation Mode Guide**: Complete simulation workflow documentation
- **Integration Documentation**: External platform integration examples
- **Server Allocation Status**: Multi-server management guide

### 🔄 Migration Notes

#### Compatibility
- ✅ Fully compatible with existing MatchZy configurations
- ✅ All original features and commands preserved
- ✅ Existing match JSON configurations work unchanged
- ✅ Drop-in replacement for standard MatchZy

#### New Requirements
- Simulation mode requires `"simulation": true` in match config
- Event delivery requires `remote_log_url` configuration
- Tournament status convars are always active (no opt-out)

#### Breaking Changes
None for standard usage. Simulation features are opt-in via match configuration.

### 🙏 Credits

This fork is maintained by **[sivert-io](https://github.com/sivert-io)** for the **[MatchZy Auto Tournament](https://github.com/sivert-io/matchzy-auto-tournament)** platform.

Built on the excellent foundation of **[Original MatchZy](https://github.com/shobhit-pathak/MatchZy)** by **WD-** (shobhit-pathak).

### 🎯 Target Use Cases

- **Tournament Platforms**: Automated match orchestration across multiple servers
- **Bracket Pre-computation**: Simulate tournament outcomes at high speed
- **Integration Testing**: Fast validation of tournament platform integrations
- **Server Management**: Coordinated updates across tournament server fleets
- **Analytics**: Rich event streams for match statistics and player tracking

---

## Original MatchZy Versions (0.8.x and earlier)

The following versions are from the original MatchZy plugin by WD- before the Enhanced fork.

---

# 0.8.27

#### November 27, 2025

- Demo recording improvements
- Auto-kick players after series end
- Enhanced server cleanup

# 0.8.26

#### November 27, 2025

- Enhanced demo upload with debug logging
- Added chat notifications for demo upload
- Improved demo upload reliability

# 0.8.25

#### November 20, 2025

- Refactored match report command for improved async handling
- Better error handling in match reports

# 0.8.24

#### November 15, 2025

- Bug fixes and stability improvements

# 0.8.23

#### November 15, 2025

- Bug fixes and stability improvements

# 0.8.22

#### November 15, 2025

- Enhanced match report upload functionality

# 0.8.20

#### November 15, 2025

- Added match report upload functionality

# 0.8.19

#### November 15, 2025

- Implemented match report command
- Enhanced player connection tracking

# 0.8.18

#### November 14, 2025

- Added version display commands
- Updated dependencies

# 0.8.17

#### November 9, 2025

- Added tournament status ConVars
- Automated release system
- Enhanced release process

# 0.8.16

#### October 26, 2025

- Added event logging improvements
- Enhanced backup management logging

---

# 0.8.15

#### October 26, 2025

- Fixed the /noclip command without any permissions.
- Updated pt-BR translation.
- Fixed database schema creation error with MySQL.

# 0.8.14

#### October 21, 2025

- Bumped CSS Version.
- Added .savepos and .loadpos commands in practice mode.
- Fixed noclip not getting disabled after switching out from practice mode.
- Fixed issues with rethrow of smoke / molly / nade.
- Fixed .rethrow always throwing incendiary instead of molotov. 
- Fixed `RemoteLogHeaderValue` value while resetting the match.

# 0.8.13

#### September 03, 2025

- Fixed coach bomb bug and updated CSS version.
- Added `matchzy_demo_recording_enabled` convar to toggle demo recording.
- Fixed the Map Winner Logic in MapWinner event
- Fixed first Map Name in database stats

# 0.8.12

#### August 25, 2025

- Updated CSS Version to fix `.last` and `.throw` commands.

# 0.8.11

#### August 9, 2025

- Updated CSS Version
- Fixed SmokeGrenadeProjectile Signatures

# 0.8.10

#### May 12, 2025

- Fixed decoy airtime in Practice Mode. (Switched `EventDecoyDetonate` to `EventDecoyStarted` so it takes the air time once it lands, and not when the decoy finishes)
- Added `.ct` and `.t` command for side selection after knife round.
- Rename `ive_wingman_override.cfg` to `live_wingman_override.cfg`
- Fixed es-ES.json

# 0.8.9

#### April 3, 2025

- Fixed issue with EventPlayerChat (. commands) post CSSharp v315 update.

# 0.8.8

#### January 1, 2025

- Fixed issue with !pause command where non-admin players were not able to take pauses when `matchzy_tech_pause_flag ""` was set.

# 0.8.7

#### December 4, 2024

- Fixed backup / restore on Windows.
- Dryrun will now have random competitive spawns rather than same spawns every time.
- Made `.pause` / `.tech` toggleable. Use `matchzy_enable_tech_pause` convar to toggle.
- Updated pt-PT translation.
- Fixed live_override

# 0.8.6

#### September 13, 2024

- Improvements in coach, now coaches will spawn on the fixed defined spawn to avoid spawning and getting stuck with the players. Spawns will be defined in `addons/counterstrikesharp/plugins/MatchZy/spawns/coach/<map_name>.json`. Each map will have its json file, in which there will be 2 keys, "3" and "2". 3 -> CT, 2 -> T and the values will be an array of Vector and QAngle objects.
- Added `.showspawns` and `.hidespawns` command for Practice mode to toggle highlighting of competitive spawns. (Image attached)
- Removed auto-join of players in match setup which was causing players to spawn under the ground.
- Added `.rr` alias for `.restart` command.

# 0.8.5

#### August 27, 2024

- Added `matchzy_match_start_message` convar to configure message to show when the match starts. Use $$$ to break message into multiple lines.
- Some improvements and guard checks in coach system
- Fixed `matchzy_hostname_format` not getting disabled on setting its value to ""
- Fixed winner side in `round_end` event

# 0.8.4

#### August 27, 2024

- Fixes in coach system, where players would spawn into each other.
- Improved backup loading (teams will be locked automatically with the first restore if match setup was being used.)
- Fixed veto bug where ready system would not work after the veto.
- Updated Uzbek translations 

# 0.8.3

#### August 25, 2024

- Fixed issues with backup restore where `.restore` command would show as round restored, but nothing happened. (Improved file naming and backup saving logic)
- Updated live.cfg as per new rules (mp_team_timeout_max 3; mp_team_timeout_ot_max 1; mp_team_timeout_ot_add_each 1)
- Added css_globalnades alias (!globalnades) for css_save_nades_as_global / .globalnades

# 0.8.2

#### August 25, 2024

- Added capability to have multiple coaches in a team.
- Coaches will now be invisible, they will drop the bomb on the spawn if they get it and will die 1 second before freezetime ends.
- If a match is loaded, player will directly join their respective team, skipping the join team menu.
- Fixed a bug where loading a saved nade would make the player stuck.
- Added `matchzy_stop_command_no_damage` convar to determine whether the stop command becomes unavailable if a player damages a player from the opposing team.
- `.map` command can now be used without "de_" prefix for maps. (Example: .map dust2)

# 0.8.1

#### August 17, 2024

- Added matchzy_enable_damage_report convar to toggle damage report after every round.
- Fixed bad demo name formatting.
- Updated Uzbek translations.

# 0.8.0

#### August 17, 2024

- Improved backup and restore system. (Added matchzy_loadbackup and matchzy_loadbackup_url commands, now round backups will be stored in .json file in csgo/MatchZyDataBackup/ directory which will have valve backup and other match config data.)
- Added matchzy_listbackups which lists all the backups for the provided matchid. By default lists backups of the current match.
- Added matchzy_hostname_format for hostname formatting.
- Improved player color smokes in practice mode
- Fixed .last grenade's player rotation
- Added switching of maps without adding de_ prefix (using .map command)
- Marked the requestBody as required: true in event_schema.yml

# 0.7.13

#### July 07, 2024

- Added alias for .savenade -> .sn, .loadnade -> .ln, .deletenade -> .dn, .importnade -> .in, .listnades -> .lin, .ctspawn -> .cts, .tspawn -> .ts
- Added smart quering for nadenames, where the closest name is being selected for loading names (.ln mid can be used to load a nade with name midflash)
- Added to allow the same lineup-name on different maps, so you can pick like b-smoke multiple times, but once per map. Updated logic for savenade, deletenade, importnade for this.
- Added missing ! commands for listnades, importnade and deletenade.
- Changed cash_team_planted_bomb_but_defused from 800 to 600 as per the update https://store.steampowered.com/news/app/730/view/4177730135016140040
- Added "override" config for live.cfg and live_wingman.cfg (Simply create live_override.cfg and live_wingman_override.cfg in the cfg folder if you want to override any of the commands.)
- Added Uzbek, Japanese, Hungarian and Traditional Chinese translations


# 0.7.12

#### June 27, 2024

- Removed unused cvars from cfgs which were causing the server to crash with the new CS# versions.
- Added MatchZyOnDemoUploadEnded Event ater demo is uploaded
- Fixed SendEventAsync Post failing when header is not empty with empty value
- Fixed decoy message localization id
- Made MatchID as int

# 0.7.11

#### May 19, 2024

- Improved `.help` command with better readability and updated commands
- Fixed overtime getting automatically enabled even if turned off in `live.cfg`
- Added `matchzy_show_credits_on_match_start` config convar to toggle 'MatchZy Plugin by WD-' message on match start.
- Added gradient while printing `KNIFE!` and `LIVE!` message.
- Added `.pip` alias for `.traj` command to toggle `sv_grenade_trajectory_prac_pipreview` in practice mode.

# 0.7.10

#### May 19, 2024

- Added `matchzy_smoke_color_enabled` config convar for practice mode which changes the smoke's color to player's team color (player's color seen in the radar)
- Added `.bestspawn` command which teleports you to your team's closest spawn from your current position
- Added `.worstspawn` command which teleports you to your team's furthest spawn from your current position
- Added `.bestctspawn` command which teleports you to CT team's closest spawn from your current position
- Added `.worstctspawn` command which teleports you to CT team's furthest spawn from your current position
- Added `.besttspawn` command which teleports you to T team's closest spawn from your current position
- Added `.worsttspawn` command which teleports you to T team's furthest spawn from your current position
- Practice mode will no longer have `warmup` help text.

# 0.7.9

#### May 06, 2024

- Updated `pt-BR` and `ru` translations.
- Added `.notready` alias for `.unready`.
- Added `.forceend` alias for `.restart`/`.endmatch`.

# 0.7.8

#### May 02, 2024

- Added `.solid` command in practice mode to toggle `mp_solid_teammates`.
- Added `.impacts` command in practice mode to toggle `sv_showimpacts`.
- Added `.traj` command in practice mode to toggle `sv_grenade_trajectory_prac_pipreview`.
- Fixed double prefix in damage report.
- Added commonly used aliases for multiple commands. (`.force` and `.forcestart` for force-start, `.tactics` to enable practice mode, `.noblind` to toggle no-flash in practice, `.cbot` to spawn a crouched-bot in practice.)
- Added `pt-BR` and `zh-Hans` updated translations.
- Renamed the `pt-pt` translation file to `pt-PT`
- Fixed translation in `fr`

# 0.7.7

#### April 29, 2024

- Added wingman support. Now, if `game_mode` is 2, plugin will automatically execute `live_wingman.cfg`. If `game_mode` is 1, `live.cfg` will be executed.
- Setting `wingman` as `true` in match config json will now automatically set `game_mode 2` and reload the map. Wingman toggle from G5V will now also work.
- Removed `UpdatePlayersMap` from player connect and disconnect methods to avoid it getting called multiple times on map change.
- Made `SetMatchEndData` to be an async operation.
- Added updated pt-PT translations.

# 0.7.6

#### April 28, 2024

- Added remaining strings available for translation.
- Fixed force-unpause command not working in knife round.
- Fixed `cfg` folder not available in Windows build of MatchZy with CSSharp.

# 0.7.5

#### April 27, 2024

- Upgraded CounterStrikeSharp to v217
- Fixed CFG execution on Map Start (After the latest update, CFGs were getting overriden by gamemodes cfg. Hence, added a timer to delay MatchZy's CFG execution on MapStart)
- Fixed BO2 setup, now Get5 server will be freed once the BO2 match is over

# 0.7.4

#### April 26, 2024

- Upgraded CounterStrikeSharp to v215
- Added Chinese (Simplified) translations.
- Fixed wrong `[EventPlayerConnectFull] KICKING PLAYER` during match setup.

# 0.7.3

#### April 06, 2024

- Added an automated build and release pipeline.
- Now all the players will be put in their team on veto start.

# 0.7.2

#### April 02, 2024

**Coach**

- Fixed rare case of everyone getting the C4 when coach gets the C4.

**Translation**

- Added German translations
- Added Portuguese (Brazil) translations
- Added French translations
- Added Portuguese (Portugal) translations

# 0.7.1

#### March 31, 2024

**Coach**

- Fixed coaches able to spectate other teams for 1 second on round start.
- Fixed coaches taking competitive spawns. Now coaches will be spawned in air (in non-competitive spawn).
- Fixed coaches getting weapons in freezetime and their weapons getting dropped after freezetime ends. Now they will be spawned without any weapons.
- Fixed coaches getting the C4. Now only the players will get the C4.

**Translation**

- Added translation/multi-lingual support in MatchZy. Currently only match related strings are added in the translation. There will be a folder called `lang` in which translation JSONs will be present. Currently we have the translations for English and Russian (thanks to @innuendo-code). To add more languages, create a JSON file with the language locale code (like `en.json` or `fr.json`, etc). Contribution for translations are much appreciated! :D

**Practice Mode/Match Mode**

- Fixed boost command working in match mode.
- Added `.skipveto` command which will skip veto in match setup (thanks to @lanslide-team) _(note: veto and this command are removed in the enhanced Auto Tournament fork)_
- Added complete support of `get5_status` command which will now return detailed status of the match setup (thanks to @The0mikkel)
- Added `mp_solid_teammates 1` in knife round for solid teammates.
- Disabled overtime when `.playout` command is used.

**Admin**

- Added a convar `matchzy_everyone_is_admin`, if set to `true`, all the players will be granted admin privileges for MatchZy commands. 

**CSSharp**

- Migrated the plugin to .NET8, make sure you use CS# v201 or above.

# 0.7.0

#### Feb 10, 2024

**Practice Mode**

- Added `!rethrow` command which rethrows player-specific last thrown grenade.
- Added `!last` command which teleports the player to the last thrown grenade position
- Added `!timer` command which starts a timer immediately and stops it when you type .timer again, telling you the duration of time
- Added `!back <number>` command which teleports you back to the provided position in your grenade history
- Added `!throwindex <index> <optional index> <optional index>` command which throws grenade of provided position(s) from your grenade thrown history. Example: `!throwindex 1 2` will throw your 1st and 2nd grenade. `!throwindex 4 5 8 9` will throw your 4th, 5th, 8th and 9th grenade (If you've added delay in grenades, they'll be thrown with their specific delay).
- Added `!delay <delay_in_seconds>` command which sets a delay on your last grenade. This is only used when using .rethrow or .throwindex
- Added `!lastindex` command which prints index (position) number of your last thrown grenade.
- Added grenade-specific rethrow commands like `!rethrowsmoke`, `!rethrownade`, `!rethrowflash`, `!rethrowmolotov` and `!rethrowdecoy`
- Grenades fly-time added. For example, on throwing a grenade, you'll get the message in chat: Flash thrown by WD- took 1.62s to detonate

**Match Mode**

- Fixed `.stay` and `.switch` not working in in side-selection phase after CS2's Arms Race update. 
- Added `!forceready` command which force-readies player's team (Currently works only in match setup using JSON/Get5, based on the value of `min_players_to_ready`)
- Improved `get5_status` command, which will now display proper `gamestate` and `matchid` when a match is loaded using JSON/Get5 (Thanks to [@The0mikkel](https://github.com/The0mikkel))

**Coach**

- Coaches will now die immediately after freeze-time ends, solving the problem of coach blocking players for 1-2 seconds.

# 0.6.1-alpha

#### Dec 27, 2023

- Added DryRun mode for Practice Mode. Use `.dryrun` while in practice mode to activate dryrun! Also added `dryrun.cfg` in `cfg/MatchZy/dryrun.cfg` which can be modified as per your requirements
- Added `.noflash` command in Practice Mode which will make the user immune to flashbangs. Use `.noflash` again to disable noflash.
- Added `.break` command in Practice Mode which will break all the breakable entities like glass windows, wooden doors, vents, etc
- Added `matchzy_demo_name_format` which will allow to set demo name as per the requirement. Default: `{TIME}_{MATCH_ID}_{MAP}_{TEAM1}_{TEAM2}` [Read More](https://shobhit-pathak.github.io/MatchZy/configuration/#matchzy_demo_name_format)
- Fixed players able to use `.tac` even after tactical timeouts were exhausted.

# 0.6.0-alpha

#### Dec 14, 2023

- Added support for Get5 Web panel! (G5V and G5API) (Read more at: https://shobhit-pathak.github.io/MatchZy/get5/)
What can Get5 Web Panel + MatchZy can do?

1. Create teams and setup matches from web panel
2. Support for BO1, BO3, BO5, etc with Veto and Knife Round
2. Get veto, scores and player stats live on the panel
3. Get demo uploaded automatically on the panel (which can be downloaded from its match page)
4. Pause and unpause game from the panel
5. Add players in a live game
6. And much more!!!

# 0.5.1-alpha

#### Dec 8, 2023

- Added `.boost`, `.crouchboost`, `.crouchbot` commands in Practice Mode to spawn Bot/Crouched bot and boost on it.
- Added `.ct`, `.t`, and `.spec` command in Practice Mode to switch the player in requested team
- Added `.fas` and `.watchme` command in Practice Mode which forces all players into spectator except the player who called this command
- Added `matchzy_autostart_mode` command for default launch mode of the plugin (0 for neither/sleep mode, 1 for match mode, 2 for practice mode. Default: 1)
- Added `matchzy_save_nades_as_global_enabled` config convar to save nades globally
- Added `matchzy_use_pause_command_for_tactical_pause` config convar to use `!pause` command as tactical pause
- Renamed `.knife` command to `.roundknife` and added `.rk` alias to resolve conflict with `.knife` command of other plugins
- Fixed tactical timeout force-unpausing the match on timeout end
- Fixed `matchzy_minimum_ready_required 0` not working properly on server startup
- Made `spectator` key in match setup config optional field

# 0.5.0-alpha

#### Dec 6, 2023

- Matches can now be setup using JSON file! This includes locking players to their correct team and side, setting the map(s) and configuring the game rules. Added `matchzy_loadmatch <filepath>` and `matchzy_loadmatch_url "<url>"` commands (read more at https://shobhit-pathak.github.io/MatchZy/match_setup/)
- Demos can now be uploaded to a URL once the map and recording ends. Command to setup the upload URL: `matchzy_demo_upload_url "<url>"` (read more at https://shobhit-pathak.github.io/MatchZy/gotv/#automatic-upload)
- Removed map reload on map end to avoid any issues
- Fixed issues while restoring round during halftime
- Fixed lag on round end which was due to pushing stats into the database. Now that operation is async!
- This one is not related to the working of the plugin, but we have a new documentation page! https://shobhit-pathak.github.io/MatchZy/

# 0.4.3-alpha

#### Nov 25, 2023

- A full implementation of CSSharp's admin system!
You can now fine-tune admin permissions as per your requirement
Flag-wise permissions:

  - `@css/root`: Grants access to all admin commands
  - `@css/config`: Grants access to config related admin commands
  - `@custom/prac`: Grants access to practice related admin commands
  - `@css/map`: Grants access to change map and toggle practice mode
  - `@css/rcon`: Grants access to trigger RCON commands using `!rcon <command>`
  - `@css/chat`: Grants access to send admin chat messages using `!asay <message>`

- Added `.forcepause` and `.forceunpause` commands for admins so that they can use `.pause` and `.unpause` as a player while playing (Use `.fp` and `.fup` for shorter commands)
- Added `.playout` commands to toggle Playout! (If playout is enabled, all rounds would be played irrespective of winner. Useful in scrims!). Also added `matchzy_playout_enabled_default` command to enable/disable playout by default. Default: `matchzy_playout_enabled_default false`
-  Added `matchzy_admin_chat_prefix` command to configure admin chat prefix when using `.asay <message>`. Default: `matchzy_admin_chat_prefix [{Red}ADMIN{Default}]`
- Added `.help` command to list all the available commands during that match phase
- Rounded off blind duration in practice mode to 2 decimal places.
- Added damage report for bot in practice mode (for every hit, similar to Get5 practice mode)
- Fixed CSTV bot getting kicked on adding a bot in practice server
- Improvements in handling saved nades (now JSON structure is used to manage saved nades, thanks to @DEAFPS!)
- Removed "Welcome to the server" message on joining the server

# 0.4.2-alpha

#### Nov 21, 2023

- MatchZy now supports CSSharp's admin system!
You can create a new entry in the `/addons/counterstrikesharp/configs/admins.json` file with `@css/generic` generic flag like mentioned in the below example:
```
{
  "WD-": {
    "identity": "76561198154367261",
    "flags": [
      "@css/generic"
    ]
  },
  "Another admin": {
    "identity": "SteamID 2",
    "flags": [
      "@css/generic"
    ]
  }
}
```

To maintain backwards compatibility, we still support creating admins using older method (by adding entries in `csgo/cfg/MatchZy/admins.json`), so you can choose the most convenient method according to your preference.

# 0.4.1-alpha

#### Nov 20, 2023

- Fixed a case where coaches were not swapping after halftime
- Fixed a case where round restore on halftime would swap the teams internally

# 0.4.0-alpha

#### Nov 17, 2023

- Coach system! `.coach <side>` Starts coaching the specified side. Example: `.coach t` to start coaching terrorist side!
- MySQL Database is now supported! Now same DB can be used with multiple servers! Configure `csgo/cfg/MatchZy/database.json` according to your need!
- `.spawn` command now uses competitive spawns!
- Many commands added in Practice mode: `.clear`, `.fastforward`, `.god`, `.savenade <name> <optional description>`, `.loadnade <name>`, `.deletenade <name>`, `.importnade <code>`, `.listnades <optional filter>` (Refer to [Readme](https://github.com/shobhit-pathak/MatchZy#practice-mode-commands) for their descriptions!)
- Added text message for showing blind duration by a flashbang in practice session!
- Damage report will now be shown for every opponent player (even if damage is not dealt!)

![pracrelease](https://github.com/shobhit-pathak/MatchZy/assets/140690706/533b4d4b-7f09-48ec-a16e-3c6c9a8cb591)

# 0.3.0-alpha

#### Nov 14, 2023

- Team names can now be configured using `!team1 <teamname>` and `!team2 <teamname>` command. The same will be stored in Database and CSV.
- If team names are not configured, it will be configured automatically by picking a player's name randomly from both the teams (For example, if there is a player `WD-`, their teamname will be set to `team_WD-`)
- Damage report in chat will be shown on round end (similar to Faceit!)
- Chat timer delay can now be configured using `matchzy_chat_messages_timer_delay`. Example: `matchzy_chat_messages_timer_delay 12` 
- Players can be whitelisted by adding their steam64id in `cfg/MatchZy/whitelist.cfg`. Whitelisting is a toggleable feature and can be enabled using `.whitelist`. To enable it by default, set `matchzy_whitelist_enabled_default true` in `cfg/MatchZy/config.cfg`

![image](https://github.com/shobhit-pathak/MatchZy/assets/140690706/85b64823-419c-41d2-850d-d8f88fa4a4ca)

# 0.2.0-alpha

#### Nov 5, 2023

- Practice mode added ( with `.bot`, `.spawn`, `.ctspawn`, `.tspawn`, `.nobots` and `.exitprac` commands!)
- Chat prefixes can now be configured using `matchzy_chat_prefix`. Example: `matchzy_chat_prefix [{Green}MatchZy{Default}]` (More details related to colors is present in readme and `config.cfg`)
- Added RCON command via chat! Now admins can use `!rcon <command>` in chat to trigger a command to the server!
- Fixed some bugs related to Demo recording and match pause when match was restarted using `.restart`
