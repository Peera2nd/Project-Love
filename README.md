# Project Love

Artist-first Thai/English visual novel and light relationship simulation, built with Unity **6000.3.24f1**, 2D URP.

## Open the game project

Add the **Project Love/** directory to Unity Hub. Open the project using its recorded Unity version. Unity resolves packages and generates Library locally; these caches are not part of the repository.

In Unity use **Tools > Project Love > Workspace**. Current main scene: `Assets/_Project/Scenes/Gameplay/StudioRomancePrototype.unity`. Source/asset authoring guidance and evidence limits are in [Docs](Docs/00_Agent_System/03_Handoff_And_Work_Logs.md).

## Repository contents

- `Project Love/Assets`, `Packages`, `ProjectSettings`: reproducible game sources and authored assets. Keep `.meta` files/GUIDs with assets.
- `Docs`: design, narrative, artist workflows and agent continuity. Raw machine-specific `.log` diagnostics stay local; summarized results, XML/JSON checks and captures remain tracked.
- `Art_Source`: editable/reference art sources supplied or generated for this project.
- `Exports`: saved text/translation snapshots; these do not synchronize automatically.
- `AGENTS.md`: required instructions for continuing agents.

The full campaign remains 30 configurable in-game days. Content, artist usability, translation coverage and balance have their documented approval/verification boundaries; a successful test is not owner acceptance.

## Experimental tools

New Story Workspace authoring is to be prototyped in an isolated sandbox and reviewed before integration. Experimental code must not be referenced by the main game or modify its authored sources. Generated sandbox projects, `.verify` caches and local backups are ignored. See the current [authoring plan](Docs/04_Technical_And_Tools/15_Scalable_Story_Authoring_And_Presentation_Plan.md).

## Git and large files

Use Git with Git LFS installed for the future large source formats listed in `.gitattributes`. Current small images remain normal Git objects. Do not commit Library, Temp, local saves, credentials or build caches. Never regenerate existing asset GUIDs to resolve a merge.
