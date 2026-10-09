# Fazbear Pizzeria After Hours

A first playable slice for a solo FNAF mashup: start at a title menu, buy attractions in a catalog, place them in a blueprint floorplan, buy a small cross-series animatronic roster, then survive one office night. FNAF 1 is the required Melty launch game. FNAF 2, FNAF 3, FNAF 4, Freddy Fazbear's Pizzeria Simulator, and Ultimate Custom Night are read from the player's own Steam installs. The app loads each game's executable icon at runtime. Selected FNAF 1 effects and room ambience supplied by the user are bundled in the root `sound-bank/` folder; see its `SOURCES.md`.

The rules are a compact, original simulation: the purchased roster sets night threats, mask/door actions handle close attacks, audio lure and ventilation belong to the FNAF 3-style threat, listening helps with the FNAF 4-style threat, and UCN-style AI levels range from 0 to 20. The pizzeria-management and office-night structure follows the user's requested Pizzeria Simulator feel.

## Local build

Run `scripts/preflight.ps1` before building. It validates the JSON sheets and then publishes a self-contained Windows x64 app to `build/`. The runtime uses .NET 8 WinForms and needs no separate .NET installation on a player's PC.

For local development only:

```powershell
dotnet run --project src/FazbearPizzeria.csproj -- --fnaf1 "C:\Program Files (x86)\Steam\steamapps\common\Five Nights at Freddy's"
```

The app checks the six Steam installs on launch. Melty passes the FNAF 1 folder as the `--fnaf1` argument; secondary folders are found through Steam's library list and app manifests. If any required title is unavailable, the game reports the missing title and does not silently substitute placeholder art. Character cards use extracted portrait art from the player's installed games, packaged locally in `assets/portraits/`.

## First-slice content

- Title menu, catalog purchasing, a placeable blueprint floorplan with Atmosphere/Health & Safety/Entertainment/Bonus ratings, and a scrollable, collapsible roster organized by source game. Each game section reserves twelve portrait slots; unused slots are intentionally empty for future characters.
- Staged animatronics add their own per-character Atmosphere, Health & Safety, Entertainment, and Bonus Revenue points to the pizzeria ratings. Hover a portrait for its name, countermeasure, ratings, and price; purchases require confirmation. Owned portraits show their AI level, which can be adjusted with the mouse wheel while hovering or with the on-screen step buttons.
- The catalog has four FFPS-inspired tiers: Dumpster Diver Weekly, Stan's Budget Tech, Smiles and Servos, Inc., and Rare Finds Auction. Place items to reach rating thresholds of 6, 15, and 30 to unlock later catalogs. It includes catalog items such as Paper Pals, Duck Pond, Fruity Maze Arcade, Candy Cadet, Riding Rockets, and Security Puppet, with scaled prices for this prototype.
- All 20 current attractions use their matching pictures from the Pizzeria Simulator [Catalog Mode gallery](https://freddy-fazbears-pizzeria-simulator.fandom.com/wiki/Catalog_Mode/Gallery). Each future attraction needs a matching `assets/attractions/{id}.png`; preflight checks that art before publishing.
- Base end-of-night payout is the placed-item cost divided by four (rounded down), plus $2 per Atmosphere point, $3 per Entertainment point, and $10 per Bonus Revenue point. Staged animatronics above AI 5 add up to +0.5% each at AI 20; each strike subtracts 25% from the final amount. Health & Safety offsets Liability Risk. Three risk-based checks can each charge a $100 lawsuit at day end. The Blueprint **RATING GUIDE** shows the live amounts and lawsuit odds.
- Per-character portraits: Bonnie has a hand-drawn prototype portrait because the installed FNAF 1 executable exposes only its Freddy icon. Freddy, Springtrap, and Nightmare Chica use UCN character portraits; Toy Bonnie uses his FNAF 2 custom-night portrait; Scrap Baby uses her FNAF 6 scene portrait.
- UCN-style per-character difficulty controls, with its own executable icon shown in the header.
- A main-menu Settings screen for remapping office actions, both doorway lights, the monitor, and previous/next camera controls. Bindings save locally between launches; F11 remains reserved for the window-mode toggle.
- Three local save slots on the main menu. Each slot autosaves funds, day, purchases, floorplan, roster, AI levels, and unlocked character categories. Slots can be loaded or erased with a delete confirmation.
- Animatronic game categories unlock with a one-time fee: FNAF 1 and UCN are open from the start; FNAF 2 costs $100, FNAF 3 $150, FNAF 4 $200, and Pizzeria Simulator $250. Each animatronic still has its own purchase price. The rising fees are this prototype's balance choices.
- A full-screen office monitor with 11 Pizzeria Simulator-style supply, advertising, and maintenance tasks plus a selectable FNaF 1 camera network. The raised monitor drains 1% power per second; camera mode uses another 1% per second. Loud print tasks can pull threats closer; finishing each task advances the clock, and finishing all 11 completes the shift.
- Two doors and two separately marked vent entrances. Every five seconds a door stays closed, it drains 5 power per closed door; with both doors open and monitor/lights off, power returns 3 points every three seconds. Threat warnings last 3–6 seconds.
- Animatronics begin at Show Stage and follow camera routes. Their movement opportunities use AI/20 odds (Freddy checks every three seconds; other roster characters every five); Freddy's movement pauses while the camera monitor is open. Toggle either doorway light to reveal an animatronic waiting at that door; each lit doorway uses 1% power per second.
- The supplied `sound-bank/` maps named FNAF 1 audio to matching events: the menu theme loops at the title screen, office ambience and the low fan play during nights, and the 6 AM cue leads into the win cue.
- Successful attacks take 25% off the final night payout each. Taking three strikes restarts the same day from its saved starting point, undoing purchases made that day. Each staged animatronic above AI 5 adds a proportional share of a payout multiplier, up to +0.5% at AI 20.
- A night skip test code: **←←↓↑AA**.
- A clear result and replay/new-day path.

## How to play this test slice

1. Select **SAVE SLOTS** from the title menu. Choose **NEW GAME** in one of the three empty slots, or load an existing slot. The game autosaves to that slot. Delete a slot from this screen and confirm to erase it. A new pizzeria starts with **$200** and Freddy already unlocked for your roster.
2. Buy attractions in **CATALOG** (confirm each purchase), then open **BLUEPRINT**, select each unplaced item, and click an empty floor tile. The two fixed dining tables occupy two floor tiles each. Paper Cups and Plates automatically sit on both tables, and the Sanitation Station sits beside the stage; both fixtures count toward ratings and payout without taking extra floor tiles. Use the red **X** on a placed attraction to return it to storage. Rating points unlock the next catalog tiers. Atmosphere, Entertainment, and Bonus Revenue increase the end-of-night payout; Health & Safety offsets Liability Risk.
3. Optionally open **ANIMATRONICS**, expand a game's section, and hover the portraits to see their rules and rating bonuses. Buying a character adds it to the stage; click its portrait again to remove it. Only staged, bought characters can attack that night. You can skip this for a quiet shift.
4. Select **OPEN FOR THE NIGHT**, use **RAISE MONITOR** (or M), then use the **DAILY TASKS** and **CAMERAS** tabs. Supply orders take 12 seconds, print jobs 20 seconds, and maintenance tasks 17 seconds. Once all tasks are done, the shift closes automatically.
5. Read each warning and use its countermeasure before the 3–6 second timer runs out. Nightmare Chica warns at the left door: press Q to listen, then close that door with A. Every hit takes 25% from the final payout; the third strike triggers a day restart and rolls back that day's purchases.

Enter the Konami code **↑↑↓↓←→←→BA** at any time for **$1,000**. It can be used repeatedly.

Both A (left) and D (right) controls visibly open and close their matching office doors. Z and C toggle the left and right doorway lights by default; each lit doorway reveals threats at that entrance and uses 1 power point per second. The monitor uses 1 point per second only while raised, and camera mode uses another 1 point per second only while the monitor is raised and cameras are selected. Each doorway light uses 1 point per second only while that light is on. Systems share a power meter; every five seconds a door stays closed it drains 5 points per closed door, and with doors open and accessories off, power recharges 3 points every three seconds. Empty power opens the doors and shuts off the monitor and lights. Rebind these controls, monitor, and previous/next camera keys in main-menu **SETTINGS**. A flashing warning triangle marks the door or vent a threat is approaching. Ventilation is separate, falls during vent threats, and must be at least 25% to use Springtrap's audio lure. During an active night, enter **←←↓↑AA** to skip directly to the successful shift result for testing. Raise the monitor and choose **CAMERAS** to inspect the FNaF 1-style map and its feeds.

The prep screen shows how many of the six source-game copies were found. A missing copy disables the corresponding purchase until the game is installed. Every roster entry must have its own `assets/portraits/{id}.png`; preflight checks this so future characters cannot silently ship with a game-icon placeholder.

The UI now uses recognizable Pizzeria Simulator cues: a purple-and-white checkerboard title backdrop, red-curtain character stage, catalog and blueprint screens, wood floor tiles, and a full pizzeria ratings panel. Character portraits come from their corresponding game where available, with UCN portraits as a fallback. Adding a future animatronic means adding its game-sourced portrait asset and its ratings to the data sheet. Only staged animatronics are active during the night. This is still a small test slice, not the complete roster. Credits, source license and remix permission still need to be agreed before a Melty listing is created.
