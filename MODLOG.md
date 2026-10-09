# Deltarune × FNAF mashup research log

## Goal

Research a Melty-publishable mashup with Deltarune as the primary game and Five Nights at Freddy's as the companion. The current concept is a short adventure through several Dark World rooms, party dialogue, classic animatronics, and alternating Deltarune-style battles with FNAF-style night-shift gameplay. First target is solo; co-op is explicitly deferred until the solo version proves worthwhile.

## Current state

- No mashup project or source code was present in the workspace when research resumed.
- `work/universal-modder` is the research toolkit clone. No game install files have been changed.
- Melty account has no existing draft for this project (`list_my_mods`).
- A first solo mechanics prototype now exists at `solo-slice/index.html`. It has not been opened or played in a browser; there is no Melty package or gameplay verification yet.

## Melty research (2026-10-09)

- Deltarune slug `deltarune`: GameMaker; no standard mod folder or loader that Melty installs. Melty supports a `standalone` mode where a custom program is installed to `{managed}` and receives the primary game's folder as `{game}`. Paid releases require review because terms are unclear.
- FNAF slug `five-nights-at-freddys`: catalog engine `other`; no standard mod folder or Melty-installed loader. Melty likewise supports passing its folder to a standalone program. Paid releases require review.
- `search_mashups` found no Deltarune × FNAF mashup. The closest install-pattern example is [Five Nights at Fazcraft](https://melty.gg/m/five-nights-at-fazcraft): Minecraft is primary and FNAF companion; Melty passes FNAF's folder as `FAZCRAFT_FNAF_DIR`, and its current recipe is one-click. This demonstrates the companion-folder pattern, not a Deltarune integration.
- Game compatibility results did not flag account or anti-cheat risks for either game. Recent install reports were mostly download timeouts/rate limits and low disk space; no gameplay incompatibility finding was returned.

## Installed game fingerprints (read-only)

- Deltarune: Steam app 1671210, local executable version 1.0.0.2, Steam build ID 24484059, path `C:\Program Files (x86)\Steam\steamapps\common\DELTARUNE`.
- Its root has a 2.95 MB `data.win` plus `chapter1_windows` through `chapter5_windows`, with a `data.win` in each chapter folder. All six data files begin with GameMaker `FORM`; a chunk walk completed at the end of every file, and each has nonempty `CODE`, `VARI`, and `FUNC` chunks. The official UndertaleModTool CLI 0.9.2.0 then loaded Chapter 1 and Chapter 5 successfully: `Is GMS2 - True`, `Is YYC - False`, bytecode version 17. It reported real code, sprite, room, and audio counts, confirming that these exact files can be inspected with the GML VM route.
- FNAF: Steam app 319510, local executable version 1.131, Steam build ID 808996, path `C:\Program Files (x86)\Steam\steamapps\common\Five Nights at Freddy's`. The install contains only `FiveNightsatFreddys.exe` (231,442,963 bytes); no loose asset archive was present. The executable contains a Clickteam marker. Clickteam's interview with Scott Cawthon identifies FNAF as made with Clickteam Fusion 2.5 ([source](https://www.clickteam.com/clickteam-blog?p=interview-of-the-author-of-a-top-paid-game-in-appstore)).
- Universal Modder's `um scan` misclassified FNAF as unknown native and warned the folder was nearly empty because it does not recognize this embedded Clickteam build. Treat that output as a detector limitation, not evidence that the game has no assets.

## Full route under consideration

Melty's supported route is a self-contained standalone mashup program that receives both installed folders. It would read Deltarune's GameMaker data and FNAF's embedded Clickteam assets from the players' copies, then implement a compact combined game loop. FNAF's night-shift rules and Deltarune's battle/adventure behavior would be reimplemented in the new runtime; neither original game process is known to expose a loader or interop API for this crossover. Do not ship either game's executable, data files, extracted assets, or decompiled game code.

This is materially different from a mod running inside Deltarune. If the user requires the mashup to run inside the original Deltarune process, the lack of a Melty-installed loader is a major blocker and must be researched separately before choosing that route.

## Main risks and unresolved work

1. UndertaleModTool successfully opened the user's installed Chapter 1 and Chapter 5 data files read-only. A runtime importer for selected resources is still unimplemented.
2. FNAF stores its assets inside a large Clickteam executable. NebulaFD successfully parsed the exact FNAF 1.131 executable as Fusion 2.5 build 284 and read its frames, image bank, and sound bank. Its Asset Dumper offers an all-assets task; this was deliberately not run to avoid extracting the full game. Selective runtime extraction and resource permissions remain unverified. NebulaFD requests .NET 6; it ran here on installed .NET 8 using `DOTNET_ROLL_FORWARD=Major`. Do not depend on full-game source output or redistribute extracted content.
3. The first solo mechanics prototype is implemented in `solo-slice/index.html`: exploration, encounter, ACT/SPARE, dodge, cameras, doors, power, and a short night timer. Its game art is drawn from original geometric shapes and is not imported from either install. The folder selectors check names only. Browser play and the Melty package are unverified.
4. The originally discussed scope (several rooms, multiple animatronics, party dialogue, Deltarune-style battles, and FNAF night shifts) is much larger than a first vertical slice. Play and review this small solo loop before widening.
5. Co-op is deferred by user direction. Revisit only after the solo version is evaluated; a later co-op scope would need a real connection method and player limit.
6. Melty one-click status is not verified for a package; no package or recipe exists.

## Toolkit note

`um kb search` could not run because the bundled Python environment lacks PyYAML. The relevant knowledge-base index entries and GameMaker/Clickteam references were read directly instead. `um scan` completed for both install folders.

Downloaded the official UndertaleModTool CLI 0.9.2.0 and the September 2026 NebulaFD release under `work/tools` for read-only inspection. No original game file was edited. No FNAF image or audio asset was exported.
