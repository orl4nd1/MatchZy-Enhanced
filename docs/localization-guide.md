# Localization Guide - v1.3.0

This guide helps translators add support for the new Enhanced Features in their language.

## Overview

MatchZy Enhanced v1.3.0 adds 21 new localization strings. These strings need to be translated into all supported languages:

- 🇩🇪 German (`de.json`)
- 🇪🇸 Spanish (`es-ES.json`)
- 🇫🇷 French (`fr.json`)
- 🇭🇺 Hungarian (`hu.json`)
- 🇯🇵 Japanese (`ja.json`)
- 🇧🇷 Portuguese BR (`pt-BR.json`)
- 🇵🇹 Portuguese PT (`pt-PT.json`)
- 🇷🇺 Russian (`ru.json`)
- 🇺🇿 Uzbek (`uz.json`)
- 🇨🇳 Chinese Simplified (`zh-Hans.json`)
- 🇹🇼 Chinese Traditional (`zh-Hant.json`)

## New Strings to Translate

### Auto-Ready System (1 string)

```json
"matchzy.autoready.markedready": "You have been automatically marked as ready. Type .unready if you are not ready."
```

**Translation notes:**
- Inform player they're auto-ready
- Tell them how to opt-out (`.unready`)
- Friendly, informative tone

---

### Knife Round & Side Selection (4 strings)

```json
"matchzy.knife.sidedecisionpendingwithtimer": "{green}{0}{default} won the knife round. They have {1} seconds to type {green}.stay{default} or {green}.switch{default}."
```
- `{0}` = Team name
- `{1}` = Seconds remaining
- Keep color codes intact

```json
"matchzy.knife.timerexpiredrandomstay": "{green}{0}{default} did not choose a side. Random selection: staying on current sides."
```
- `{0}` = Team name
- Explain random outcome (stayed)

```json
"matchzy.knife.timerexpiredrandomswap": "{green}{0}{default} did not choose a side. Random selection: swapping sides."
```
- `{0}` = Team name
- Explain random outcome (swapped)

---

### Enhanced Pause System (5 strings)

```json
"matchzy.pause.pausedthematchwithlimit": "{green}{0}{default} has paused the match ({1} pauses remaining). Type .unpause to unpause the match."
```
- `{0}` = Team name
- `{1}` = Pauses remaining (number)
- Show both info and instruction

```json
"matchzy.pause.teamunpausedthematch": "{green}{0}{default} has unpaused the match!"
```
- `{0}` = Team name
- Celebrate/announce the unpause

```json
"matchzy.pause.nopausesleft": "{green}{0}{default} has no more pauses left ({1} max)."
```
- `{0}` = Team name
- `{1}` = Maximum pauses allowed
- Error message

```json
"matchzy.pause.timeoutexpired": "Pause timeout expired, resuming match."
```
- Neutral announcement
- No parameters

---

### .gg Command (6 strings)

```json
"matchzy.gg.disabled": "The .gg command is disabled on this server."
```
- Error message when feature disabled

```json
"matchzy.gg.matchnotlive": "Match is not live. You can only use .gg during a live match."
```
- Error: wrong match phase

```json
"matchzy.gg.mustbeonteam": "You must be on a team (CT or T) to use .gg."
```
- Error: player is spectator

```json
"matchzy.gg.alreadyvoted": "You have already voted to end the match."
```
- Error: duplicate vote

```json
"matchzy.gg.playervoted": "{green}{0}{default} from {green}{1}{default} voted to end the match ({2}/{3} votes)."
```
- `{0}` = Player name
- `{1}` = Team name
- `{2}` = Current votes
- `{3}` = Votes needed
- Announce vote progress

```json
"matchzy.gg.thresholdmet": "{green}{0}{default} has reached the threshold to end the match. The opposing team wins!"
```
- `{0}` = Forfeiting team name
- Announce match end

---

### FFW System (4 strings)

```json
"matchzy.ffw.started": "{green}{0}{default} has left the server. Forfeit timer started: {1} minute(s) remaining."
```
- `{0}` = Missing team name
- `{1}` = Minutes until forfeit
- Initial warning

```json
"matchzy.ffw.warning": "{yellow}Warning:{default} {green}{0}{default} still missing. Forfeit in {1} minute(s) if no one rejoins."
```
- `{0}` = Missing team name
- `{1}` = Minutes remaining
- Repeated warning

```json
"matchzy.ffw.cancelled": "{green}{0}{default} returned! Forfeit timer cancelled."
```
- `{0}` = Returned team name
- Positive message

```json
"matchzy.ffw.executed": "{green}{0}{default} forfeited the match. {green}{1}{default} wins by forfeit!"
```
- `{0}` = Forfeiting team name
- `{1}` = Winning team name
- Final announcement

---

## Translation Guidelines

### General Rules

1. **Preserve placeholders** - Keep `{0}`, `{1}`, etc. in the same order
2. **Keep color codes** - Don't translate `{green}`, `{default}`, etc.
3. **Match tone** - Informative, professional, but friendly
4. **Test in-game** - Verify text fits on screen
5. **Use proper grammar** - Native-level quality expected

### Color Codes Reference

- `{Default}` - Default chat color
- `{Green}` - Success/positive/team names
- `{Red}` - Errors/warnings
- `{Yellow}` - Attention/warnings
- `{Lime}` - Highlights
- Other colors available but rarely used

### Command References

When mentioning commands in translations:
- Keep the `.` prefix: `.ready`, `.gg`, `.unpause`
- These are English commands and should NOT be translated
- Example: "Type .unready" → "Tapez .unready" (French)

### Number Formatting

- Use proper singular/plural forms in your language
- Example EN: "1 minute" vs "2 minutes"
- Example FR: "1 minute" vs "2 minutes"
- Example RU: "1 минута" vs "2 минуты" vs "5 минут"

---

## Translation Template

Copy this template to your language file:

```json
  "matchzy.autoready.markedready": "",
  
  "matchzy.knife.sidedecisionpendingwithtimer": "",
  "matchzy.knife.timerexpiredrandomstay": "",
  "matchzy.knife.timerexpiredrandomswap": "",
  
  "matchzy.pause.pausedthematchwithlimit": "",
  "matchzy.pause.teamunpausedthematch": "",
  "matchzy.pause.nopausesleft": "",
  "matchzy.pause.timeoutexpired": "",
  
  "matchzy.gg.disabled": "",
  "matchzy.gg.matchnotlive": "",
  "matchzy.gg.mustbeonteam": "",
  "matchzy.gg.alreadyvoted": "",
  "matchzy.gg.playervoted": "",
  "matchzy.gg.thresholdmet": "",
  
  "matchzy.ffw.started": "",
  "matchzy.ffw.warning": "",
  "matchzy.ffw.cancelled": "",
  "matchzy.ffw.executed": ""
```

---

## Example Translations

### German (de.json)

```json
"matchzy.autoready.markedready": "Sie wurden automatisch als bereit markiert. Geben Sie .unready ein, wenn Sie nicht bereit sind.",
"matchzy.gg.playervoted": "{green}{0}{default} von {green}{1}{default} hat für das Spielende gestimmt ({2}/{3} Stimmen).",
"matchzy.ffw.warning": "{yellow}Warnung:{default} {green}{0}{default} fehlt weiterhin. Forfeit in {1} Minute(n), falls niemand zurückkehrt."
```

### French (fr.json)

```json
"matchzy.autoready.markedready": "Vous avez été automatiquement marqué comme prêt. Tapez .unready si vous n'êtes pas prêt.",
"matchzy.gg.playervoted": "{green}{0}{default} de {green}{1}{default} a voté pour mettre fin au match ({2}/{3} votes).",
"matchzy.ffw.warning": "{yellow}Attention:{default} {green}{0}{default} toujours absent. Forfait dans {1} minute(s) si personne ne revient."
```

### Spanish (es-ES.json)

```json
"matchzy.autoready.markedready": "Has sido marcado automáticamente como listo. Escribe .unready si no estás listo.",
"matchzy.gg.playervoted": "{green}{0}{default} de {green}{1}{default} votó para terminar el partido ({2}/{3} votos).",
"matchzy.ffw.warning": "{yellow}Advertencia:{default} {green}{0}{default} sigue ausente. Forfeit en {1} minuto(s) si nadie regresa."
```

---

## Testing Your Translation

1. Add your translations to the appropriate `.json` file
2. Restart the server
3. Enable the feature in config
4. Test each string appears correctly in-game
5. Verify colors render properly
6. Check text doesn't overflow chat area
7. Test with different placeholder values (team names, numbers)

---

## Submitting Translations

### Option 1: Pull Request
1. Fork the repository
2. Add translations to `lang/<your-language>.json`
3. Test thoroughly
4. Submit PR with title: "Localization: Add v1.3.0 strings for [Language]"

### Option 2: Issue
1. Open a GitHub issue
2. Title: "Translation: v1.3.0 strings for [Language]"
3. Paste your translated JSON
4. We'll review and merge

---

## Translation Status

Track community progress:

- [x] 🇬🇧 English (`en.json`) - Complete ✅
- [ ] 🇩🇪 German (`de.json`) - **Needs translation**
- [ ] 🇪🇸 Spanish (`es-ES.json`) - **Needs translation**
- [ ] 🇫🇷 French (`fr.json`) - **Needs translation**
- [ ] 🇭🇺 Hungarian (`hu.json`) - **Needs translation**
- [ ] 🇯🇵 Japanese (`ja.json`) - **Needs translation**
- [ ] 🇧🇷 Portuguese BR (`pt-BR.json`) - **Needs translation**
- [ ] 🇵🇹 Portuguese PT (`pt-PT.json`) - **Needs translation**
- [ ] 🇷🇺 Russian (`ru.json`) - **Needs translation**
- [ ] 🇺🇿 Uzbek (`uz.json`) - **Needs translation**
- [ ] 🇨🇳 Chinese Simplified (`zh-Hans.json`) - **Needs translation**
- [ ] 🇹🇼 Chinese Traditional (`zh-Hant.json`) - **Needs translation**

---

## Questions?

For translation questions or assistance:
- Open a GitHub issue
- Join community Discord
- Tag maintainers in PR

**Thank you for contributing to MatchZy Enhanced!** 🎉

---

<div align="center">

**Native speakers preferred** • **Quality over speed** • **Test before submitting**

</div>
