# Deltarune × FNAF mashup: research plan

## Install fingerprints

| Game | Install/version | Engine evidence | Melty route |
|---|---|---|---|
| Deltarune (primary) | Steam app 1671210, build 24484059, executable 1.0.0.2 | GameMaker; `data.win` and all five chapter data files have nonempty GML code chunks | No Melty-installed loader; standalone mode can receive the Deltarune folder |
| Five Nights at Freddy's (companion) | Steam app 319510, build 808996, executable 1.131 | Clickteam Fusion 2.5 per Clickteam's interview; content appears embedded in the 231 MB executable | No Melty-installed loader; standalone mode can receive the FNAF folder |

## Route under consideration

Build a self-contained standalone runtime and converters that read the player's installed copies at launch. Deltarune supplies its real character/world material; FNAF supplies its real animatronic material. Recreate the needed adventure, battle, and night-shift rules in the mashup runtime. Keep all original game assets and code out of the release.

This route best matches Melty's explicit `standalone` path for games without loaders. It is not a modification running inside Deltarune's original process. A direct Deltarune mod would require a separate investigation of how to load FNAF companion data without a Melty-installed loader.

## Solo slice implementation

Created `solo-slice/index.html`, a self-contained browser prototype with a title screen, a moveable Dark World backstage room, a turn-based encounter with ACT/SPARE and a dodge phase, and a short camera/door night shift. The visuals are original geometric placeholders drawn by canvas code. The optional folder selectors check only expected filenames; they do not read or extract game content.

This is a mechanics prototype, not yet a Melty package or the proposed runtime asset importer. Browser play has not been manually verified. Do not expand the cast, add rooms, or begin co-op until this solo loop is played and reviewed. Next research step is a safe selective runtime/import approach for Deltarune and FNAF assets; NebulaFD's all-assets dumper was intentionally not run.

## Co-op

First target is solo. Co-op is deferred until the solo version proves worthwhile, per user direction. Do not spend the first implementation on networking.

## Confirmed parser research

- UndertaleModTool CLI 0.9.2.0 successfully loaded the installed Deltarune Chapter 1 and Chapter 5 `data.win` files as GameMaker 2 VM builds (not YYC), bytecode 17. The six local `data.win` files have nonempty code chunks.
- NebulaFD's September 2026 CLI successfully read the installed FNAF 1.131 executable as a Clickteam Fusion 2.5 build 284 and parsed the frame, image, and sound banks. Asset export has not been run.

## Still unverified

- A clean, legal runtime method for importing selected assets from both games without distributing those assets or decompiled game code.
- Runtime size, startup time, one-click recipe, multiplayer transport, and end-to-end gameplay.
- Credits, content license, remix permission, listing text, and gameplay capture.
