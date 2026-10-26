# 00 — Art Pipeline and Specifications

**Status:** production image specifications remain proposed. Sandbox Thai UI/font and supplied room/character/item imports/rendering are verified in the isometric prototype. Final production art/CG quality is not approved; see the [room guide](../04_Technical_And_Tools/02_Isometric_Prototype.md). [Master Decisions](../01_Game_Design/00_Master_Vision_And_Decisions.md) owns approval of production scope. Listed CGs are proposed content, not delivered assets.

## Isometric mockup delivery — Session 017

Use the [Isometric Mockup Asset Brief](01_Workspace_Mockup_Asset_Brief.md): eight core PNGs demonstrate two scenes, two poses/locations, VN expression, clerk shopping and item pickup; four files can start the workspace alone. Room-character sprites are local transparent poses; only scene background/foreground layers use full-frame registration. This supersedes Session 016 static seat exports. Session 019 integrates supplied mockup art: 1024x1024 room variants over black, local character poses and VN expressions. Exact placement/occlusion and final production art remain iterative.

## Additional team mockups — Session 023

[Grace/Ploy asset request](02_Team_Mockup_Asset_Request.md): four required exports (one room work pose and one VN Normal each), eight with optional relaxed poses/Smile. Existing room/Plaifah assets stay in use; no new background, board artwork or CG required. Request only; no new art generated/imported in this session.

## Resolution

- Reference UI/game resolution: `1920 x 1080`, 16:9. Owner-supplied room textures are `1024 x 1024` with alpha, aspect-preserved over a code-generated black backdrop; they are not stretched CGs.
- Suggested source canvas: `1920 x 1080` or `2560 x 1440`; `3840 x 2160` may suit CG zoom/pan.
- Verify scaling, cropping and legibility in the eventual scene; source resolution alone does not guarantee final quality.

## Character sprites

- VN character canvas: suggested `1500 x 2000`, transparent PNG. Isometric room poses use a separate local canvas, suggested `1024 x 1024`, with data-authored placement/contact references; see the mockup brief. Editable masters may use PSD/PSB.
- Store standing sprites in `_Project/Art/Characters/Sprites/`; portraits in `Characters/Portraits/`.
- Initial expression keys: **Normal, Smile, Angry, Sad, Blush, Surprised**.

| Descriptive alias | Initial sprite key |
| --- | --- |
| Stressed / Crying | Sad |
| Cold | Angry |
| Shocked | Surprised |

Aliases do not automatically add production artwork. Add a distinct expression to CharacterDataSO and the art checklist before using it as a new asset key.

## Backgrounds and CGs

- Suggested background/CG size: `1920 x 1080` or `2560 x 1440` with crop/pan allowance.
- Reuse a main scene with lighting variants: `_Morning`, `_Evening`, `_Night`.
- Draft phase mapping: Morning/Daytime -> `_Morning`; Evening -> `_Evening`; Night -> `_Night`.
- Store backgrounds in `_Project/Art/Backgrounds/`, CGs in `_Project/Art/CGs/`.
- The 30-day campaign adds pacing space, not an automatic CG quota increase. Current proposal: four CGs per draft heroine; distinguish slice essentials from full-game scope.

## Icons, UI, fonts and audio

- Item/skill icons: suggested `256 x 256` or `512 x 512`, transparent PNG; `_Project/Art/Items/` and `_Project/Art/UI/`.
- UI graphics and VFX: their corresponding Art folders.
- Thai TMP font assets: `_Project/Resources/Fonts/`. Verify glyph coverage, combining marks, line wrapping, rich text and typewriter reveal in the actual UI.
- Audio: `_Project/Audio/BGM/`, `SFX/`, `Voice/`. Full voice versus short barks remains a production choice, not an existing deliverable.
- Artist-facing instructions/labels in tools and story/character briefs remain Thai-friendly; this backend production specification is English.
