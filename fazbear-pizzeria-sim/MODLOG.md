# Fazbear Pizzeria After Hours — build journal

## Agreed slice

- A single-player, offline pizzeria management day followed by one office night.
- Players buy a small sample animatronic roster; those purchases become threats during the night.
- FNAF 1 is the Melty primary/launch game. Pizzeria Simulator is not a Melty host, but its pizzeria and office-night ideas anchor the game.
- FNAF 2 contributes the mask, FNAF 3 the audio lure and ventilation, FNAF 4 listening/door decisions, and Ultimate Custom Night the 0–20 AI settings.
- This is a test version; more animatronics are intended later.
- The first pizzeria day starts with $500. Placed attractions return 25% of cost after the shift; surviving the office night grants no cash bonus. The Konami code **↑↑↓↓←→←→BA** adds $1,000 per use.
- The office door is a visible state with a draining/recharging door-power meter; ventilation is separate and supports the audio-lure rule. The camera is one fixed East Hall view for now.

## Route

Build a Windows standalone rewrite that receives the player's FNAF 1 folder from Melty and reads the other installed Steam game executables at runtime. The game writes no changes into any source game folder and ships no source-game files. Each title's executable icon is used directly from that installed executable as its roster art; the slice implements a small original simulation of the named game rules. This differs from the closest Melty example, Five Nights at Cuphead, by using management as the daytime loop and a cross-series purchased roster as the night threats.

## Files and checks

- Source-of-truth design rows: `sheets/games.json`, `sheets/animatronics.json`, and `sheets/systems.json`.
- `src/` loads these sheets as typed records. `scripts/preflight.ps1` checks cells and references before each build.
- Target OS: Windows x64. .NET 8 is bundled by self-contained publish; players do not install a runtime.
- Required owned games: FNAF 1, 2, 3, 4, Freddy Fazbear's Pizzeria Simulator, and Ultimate Custom Night.
- Multiplayer: none in this first version.

## Known publishing constraint

Melty recognizes FNAF 1, FNAF 2, FNAF 3, FNAF 4 and Ultimate Custom Night, but `search_games` did not return Freddy Fazbear's Pizzeria Simulator by its title, and the draft tool's `gameSlugs` schema caps a listing at four games. Do not substitute an unrelated game slug. Recheck this before any draft or release; keep the listing unpublished if Melty still cannot represent the actual six-game set.

## Verification log

- Melty `game_info` confirms FNAF 1 can launch a standalone program that receives `{game}` and has no one-click loader; this is the appropriate route for this concept.
- Melty `mashup_info` verified Five Nights at Cuphead as the closest FNAF 1/FNAF 2 precedent: standalone, primary FNAF 1, secondary games found from Steam installs.
- All six requested installs are present under the default Steam common folder on this PC. Their executable filenames and sizes are recorded from the filesystem during development; none are copied into this project.
- Preflight passed with all six game rows, five animatronics, and six systems; the self-contained Windows x64 publish succeeded.
- Startup smoke check recognized all 6/6 locally installed game copies after mapping the `steamAppId` sheet field correctly and supporting FNAF 4's actual Steam install-folder name.
- Normal launch displayed the guided pizzeria screen. `--capture-demo` launched the timed office night; its current controls and alert instructions were visually checked. A full night has not yet been played through manually.
- Current local UCN installation has only the game executable in its install folder; no loose character-portrait files were available to load. The slice continues using genuine source-game executable icons rather than substituting a repeated or mismatched UCN portrait.
- Economy and office feedback update: $500 opening budget, a rendered open/closed right door, door-power drain/recharge/auto-open, and separate ventilation drain/reset/audio-lure threshold. (The earlier clear-night cash bonus was later removed at the user's request.)
- The preflight now copies and verifies all three JSON sheets beside the single-file executable, so a clean publish starts without relying on stale files in `build/`.
- Fresh publish and launch succeeded; the live prep screen shows $500 and the two affordable starting choices.
- Added a title menu that opens the pizzeria builder, with three visible attraction slots and buyable attractions. The roster remains on a separate tab.
- The office now has independently operable left and right doors, explicit A/D labels, shared power drain/recharge, and a flashing red exclamation triangle over the threatened doorway until the attack resolves. The night panel tells the player which key controls each door.
- Kept the single East Hall camera as a stated test-slice limitation. Installed source copies still expose executable icons only; character-specific custom-night portraits are not loose assets and have not been extracted.
- Fresh preflight and self-contained Windows x64 publish passed after the menu, builder, and door-alert changes. The night has not been manually played through again.
- Progression fix: a night can start with zero purchased animatronics (including when all $500 went to attractions). The office explains that it is a quiet shift until 6 AM; the README documents the option.
- AI difficulty step buttons now render centered ASCII minus/plus marks instead of passing their glyphs through the wide generic button layout.
- Attraction returns are paid at the end of the first successful shift that ends that day, at 25% of purchase price rounded down to whole dollars. Failed or abandoned shifts pay nothing; retries can collect once upon a later win.
- Visual pass: fully spelled out Atmosphere, Health & Safety, Entertainment, and Bonus Revenue; replaced the generic teal/serif treatment with a Pizzeria Simulator inspired purple checkerboard title, red stage curtains, source-game icon lineup, wood blueprint tiles, purple/brown interface palette, and an office poster/fan/cup detail pass.
- Economy/cheat update: remove the $500 clear-night reward. The Konami sequence adds $1,000 on each completion, with key-repeat protection and an on-screen toast. The catalog now leads to a separate blueprint placement screen with simple Pizzeria Simulator-inspired ratings; only placed attractions yield the quarter-cost return after a shift.
- Removed the large center-screen motion card from the office. Added a labeled top-center vent grille and a flashing red `!` marker immediately below it while a vent-scan threat is approaching. Existing door warning triangles remain above their corresponding doors.

- UI correction: moved the roster hover details into a full-width strip between the instructions and portraits, preventing it from covering the title or clipping the description. Kept the AI controls in that strip.
- Catalog art: all 20 current attraction cards, floorplan placements, and fixtures now load their matching item pictures from the linked Catalog Mode gallery. Preflight checks every attraction image and copies them into the build. The official FFPS install contains only a packed executable, so the gallery's matching image files are used as the source.

## Rights and listing

No source-game assets are in the project. The program reads executable icons from the user's own installed copies at runtime. Credits, source-code license, and remix permission are not established yet; obtain these before making a Melty listing. Do not claim a listing is live until Melty confirms it.

- Save files: added three slots to the title menu, with new-game/load actions, explicit delete confirmation, and automatic persistence of funds, day, inventory, stage lineup, AI levels, unlocked source-game categories, and placed floorplan. Files are stored under `%LOCALAPPDATA%\FazbearPizzeriaAfterHours\save-1.json` through `save-3.json`.
- Animatronic category access now requires a one-time fee: FNAF 2 $100, FNAF 3 $150, FNAF 4 $200, Pizzeria Simulator $250; FNAF 1 and UCN begin unlocked. Individual characters keep their own purchase prices. These are balancing values for this prototype.

- Night task update: the office monitor now runs 11 timed Pizzeria Simulator-style supply, print, and maintenance tasks. Loud print work brings active threats forward; each completed task advances the shift clock and all tasks log off the shift. The night skip sequence is Left, Left, Down, Up, A, A.
- Office balancing: shortened warning windows to 3–6 seconds, added the two vent indicators, changed door drain/recharge to 5/3 power points per second, and removed attraction-name captions from placed items.
- Payout and failure update: each strike reduces the final pay by 25%; a third strike restarts the same day from its saved beginning-of-day inventory and funds. Staged animatronics above AI 5 add a proportional end-payout multiplier up to 0.5% apiece at AI 20.
