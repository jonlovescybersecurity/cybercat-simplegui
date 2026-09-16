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

- [ ] Confirm a clean build with the pinned WolvenKit submodule.
- [ ] Test read-only loading of fresh Cyberpunk 2077 2.31 saves.
- [ ] Test both base-game and Phantom Liberty saves.
- [ ] Audit save reader/writer assumptions against current WolvenKit save code.
- [ ] Update the WolvenKit pin only if current save compatibility requires it.
- [ ] Validate appearance editing.
- [ ] Validate inventory/item editing.
- [ ] Validate player stats editing.
- [ ] Validate quest facts.
- [ ] Validate vehicles and Phantom Liberty-specific data.
- [ ] Add automatic backup/safety checks before save writes.
- [ ] Refresh game-version documentation and support matrix.
- [ ] Add a Windows CI build for reproducible test artifacts.
- [ ] Produce an alpha build for manual testing before merging to `master`.

## Safety rule

Do not test early save-writing changes against valuable live saves. Duplicate a save first, preserve the original directory, and use the copy for writer tests.
