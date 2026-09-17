# CyberCAT-SimpleGUI current-game roadmap

## Goal
Bring this fork forward from the July 2025 codebase to current Cyberpunk 2077 save compatibility, including Phantom Liberty, while preserving the existing SimpleGUI workflow.

## Baseline discovered

- Development branch: `develop/current-cp2077`
- The fork already contains the July 2025 Cyberpunk 2.3 compatibility work.
- The source UI reports version `v0.28c`.
- The project targets .NET 8 (`net8.0-windows7.0`).
- WolvenKit is pinned to commit `a58dc27d8f9abcba768589af226b00ca8a9d1488` from 2025-07-22.
- The latest public Nexus package is 0.28b from 2025-07-22, while upstream source contains two later commits for stats reading, modded life paths, PS4 cleanup, and the version bump to 0.28c.
- Current Cyberpunk 2077 PC patch target: 2.31.

## Work plan

- [x] Confirm a clean build with the pinned WolvenKit submodule. `ResourceGenerator` needed an update for WolvenKit's current hash API.
- [x] Test read-only loading of fresh Cyberpunk 2077 2.31 saves. `ManualSave-26` and `AutoSave-0` parsed with the pinned submodule, including the core data accessors used by the main editor tabs.
- [x] Test read-only loading of both base-game and Phantom Liberty 2.31 saves. The no-DLC community save metadata reports PC version `2310` with no additional content IDs; it and two local EP1 saves passed the probe.
- [ ] Audit save reader/writer assumptions against current WolvenKit save code.
- [x] Assess the WolvenKit pin. No update was needed for the sampled 2.31 saves.
- [ ] Validate appearance editing.
- [ ] Validate inventory/item editing.
- [ ] Validate player stats editing.
- [ ] Validate quest facts.
- [ ] Validate vehicles and Phantom Liberty-specific data.
- [x] Add automatic backup/safety checks before save writes. Existing `sav.dat` files are replaced from a completed temporary file, with a unique backup for each replacement. Two synthetic replacements verified backup contents.
- [x] Refresh game-version documentation and support matrix.
- [x] Add a Windows CI build for reproducible test artifacts. The workflow still needs a hosted CI run.
- [x] Produce a local alpha build for manual testing before merging to `master`. The ignored `artifacts/CyberCAT-SimpleGUI-current-cp2077-alpha.zip` contains the editor and support files; manual UI and game acceptance checks remain.

## Safety rule

Do not test early save-writing changes against valuable live saves. Duplicate a save first, preserve the original directory, and use the copy for writer tests.

## Current validation limits

- The local 2.31 saves have `EP1` in `additionalContentIds`. A [community 2.31 no-DLC save](https://gtrainers.com/load/categories/savegames/cyberpunk_2077_savegame_immortality_and_insane_damage_100_completed_without_phantom_liberty_2_31/30-1-0-15083) supplied base-game coverage. Its archive is kept only in ignored local artifacts, not in the repository.
- The save probe reads files only. It does not serialize edited saves or check whether the game accepts them.
- An unchanged `ManualSave-26` was serialized to a new workspace file. It parsed again with the same 1,459 node names and order, and the main data accessors succeeded. This does not establish that edited saves load in the game.
- The no-DLC sample also serialized to a new file, parsed again with the same 422 node names and order, and passed the main data accessors.
- Disposable edits to one quest fact and one non-quest inventory quantity in the no-DLC sample serialized and reparsed with the changed value and the same 422 node names/order. A subsequent player-development probe stopped with an out-of-memory error; that edit and the Phantom Liberty edits are not yet verified. These probes do not establish in-game compatibility.
- The alpha opened its main window on a host whose `DOTNET_ROOT` points to .NET 9 after enabling major runtime roll forward. A tab-by-tab GUI load check remains pending.
- The WolvenKit pin remains at `a58dc27d8f9abcba768589af226b00ca8a9d1488` because sampled 2.31 files load successfully.

Run the probe with `dotnet run --project tools/SaveLoadProbe/SaveLoadProbe.csproj -c Release -p:Platform=x64 -- <path-to-sav.dat>`. It reports the save header version, node counts, missing required nodes, and whether appearance, inventory, stats, player development, quest facts, vehicles, and scriptable systems can be accessed.

For an unchanged writer check, run `dotnet run --project tools/SaveRoundTripProbe/SaveRoundTripProbe.csproj -c Release -p:Platform=x64 -- <input-sav.dat> <new-output.dat>`. The output path must not exist and must be outside the input save directory. The probe writes a separate file, then parses it and compares the version and node names/order.

For a disposable field edit, run `dotnet run --project tools/SaveEditProbe/SaveEditProbe.csproj -c Release -p:Platform=x64 -- <input-sav.dat> <new-output.dat> <facts|inventory|development>`. The probe writes only a new output outside the input directory and checks the changed value after reparsing. Do not load probe outputs in the game without making a separate backup of the original save directory.
