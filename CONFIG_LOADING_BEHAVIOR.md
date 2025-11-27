# MatchZy Config File Loading Behavior

This document explains when and how MatchZy loads configuration files, and how to prevent dynamically set values from being overwritten.

## Config Files Execution Timeline

### 1. Plugin Load (Initial Load)
**When:** Server starts or plugin is loaded/reloaded  
**Location:** `MatchZy.cs` line 98  
**Command:** `execifexists MatchZy/config.cfg`

```csharp
public override void Load(bool hotReload) {
    // ...
    Server.ExecuteCommand("execifexists MatchZy/config.cfg");
    // ...
}
```

**What happens:**
- Executes `cfg/MatchZy/config.cfg` if it exists
- Sets default values for all MatchZy convars
- This is the **ONLY** time `config.cfg` is automatically executed

### 2. Match Goes Live
**When:** Match starts (after ready-up)  
**Location:** `Utility.cs` - `ExecLiveCFG()`  
**Command:** `exec MatchZy/live.cfg` (or `live_wingman.cfg` for wingman)

**What happens:**
- Executes `cfg/MatchZy/live.cfg` (game settings, not MatchZy config)
- Does **NOT** execute `config.cfg`
- `live.cfg` may execute `live_override.cfg` but not the main config

### 3. Practice Mode
**When:** Practice mode starts  
**Location:** `PracticeMode.cs`  
**Command:** `exec MatchZy/prac.cfg`

**What happens:**
- Executes practice-specific game settings
- Does **NOT** execute `config.cfg`

### 4. Warmup
**When:** Warmup phase starts  
**Location:** `Utility.cs` - `StartWarmup()`  
**Command:** `exec MatchZy/warmup.cfg`

**What happens:**
- Executes warmup game settings
- Does **NOT** execute `config.cfg`

### 5. Manual Config Reload
**When:** Admin manually executes the command  
**Command:** `exec MatchZy/config.cfg`

**What happens:**
- Re-executes `config.cfg`
- **⚠️ WARNING:** This can overwrite dynamically set values!

## Protection for `demoUploadURL`

To prevent `demoUploadURL` from being overwritten when config files are loaded, MatchZy now includes protection:

### How It Works

1. **Initial Load:** When `config.cfg` is executed on plugin load, it can set `demoUploadURL` to empty string (default)

2. **Dynamic Setting:** When you set `demoUploadURL` via console command (from your web panel), it:
   - Sets the URL value
   - Marks it as "dynamically set" with a flag
   - Logs the action

3. **Config Reload Protection:** If `config.cfg` is executed again (manually or on reload):
   - If `demoUploadURL` was set dynamically AND the config tries to set it to empty, it **ignores** the config value
   - Logs a warning message
   - Preserves the dynamically set value

### Example Behavior

```bash
# Initial load - config.cfg sets it to empty (default)
[MatchZyDemoUploadURL] Demo upload URL not configured (empty). Set matchzy_demo_upload_url to enable automatic uploads.

# Your web panel sets it dynamically
matchzy_demo_upload_url "https://api.example.com/upload"
[MatchZyDemoUploadURL] Demo upload URL set to: https://api.example.com/upload

# Someone manually reloads config.cfg (which has empty value)
exec MatchZy/config.cfg
[MatchZyDemoUploadURL] Ignoring empty URL from config - demoUploadURL was set dynamically and will not be overwritten. Current value: https://api.example.com/upload
```

## Summary: When Config Files Are Executed

| Event | Config File Executed | Executes `config.cfg`? | Can Overwrite `demoUploadURL`? |
|-------|---------------------|----------------------|--------------------------------|
| Plugin Load | `config.cfg` | ✅ Yes (initial load) | ✅ Yes (initial value) |
| Match Goes Live | `live.cfg` | ❌ No | ❌ No |
| Practice Mode | `prac.cfg` | ❌ No | ❌ No |
| Warmup | `warmup.cfg` | ❌ No | ❌ No |
| Manual Reload | `config.cfg` | ✅ Yes | ⚠️ Protected (won't overwrite if set dynamically) |
| Plugin Reload | `config.cfg` | ✅ Yes | ⚠️ Protected (won't overwrite if set dynamically) |

## Best Practices

1. **Set `demoUploadURL` via Console Command:** Use `matchzy_demo_upload_url "https://..."` from your web panel/API
   - This marks it as dynamically set
   - Protected from config file overwrites

2. **Keep `config.cfg` Empty for Dynamic Values:** Leave `matchzy_demo_upload_url ""` in `config.cfg` as the default
   - This allows your web panel to set it without conflicts

3. **Monitor Logs:** Check server logs for messages like:
   - `[MatchZyDemoUploadURL] Demo upload URL set to: ...`
   - `[MatchZyDemoUploadURL] Ignoring empty URL from config...`

4. **Avoid Manual Config Reloads During Matches:** Don't run `exec MatchZy/config.cfg` while a match is active if you've set values dynamically

## Other Config Variables

Currently, only `demoUploadURL` has this protection. Other variables like:
- `matchzy_demo_path`
- `matchzy_demo_name_format`
- `matchzy_demo_recording_enabled`

...will be overwritten if `config.cfg` is reloaded. If you need similar protection for other variables, the same pattern can be applied.

