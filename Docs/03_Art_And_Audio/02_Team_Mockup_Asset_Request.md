# Team Mockup Asset Request — Grace and Ploy

Session 024, 2026-10-24. **Delivered:** all eight PNGs generated via local SD WebUI (`prefectIllustriousXL_40`), processed with `rembg`, canvas-fitted and delivered to runtime paths. Session 026 integrates all eight images into the separate Studio cast/room with phase placements and Normal/Smile VN sprites. PNG bytes/GUIDs are preserved; importer metas are normalized for Single/readable sprites. Fine seating/occlusion remains mockup work; see the [Studio guide](../04_Technical_And_Tools/03_Studio_Sprint_Prototype.md). Add two characters so Code/Art/Design can be distinguished in the same room. Keep existing Plaifah artwork and the 1024-square rooms over generated black.

## Priority and scope

- **Minimum: four PNGs** — working room pose and Normal VN sprite for each new character. Enough to test selecting each worker, their work stream and dialogue.
- **Recommended: eight PNGs** — add a relaxed room pose and Smile VN expression per character. These support phase swaps and reactions without requiring a large expression set.
- Sketch/flat-color mockups are sufficient. Reuse the supplied Plaifah style, proportions and isometric projection rather than increasing polish for only the new cast.
- No new background, player sprite, custom board/card artwork, animation, CG, illness scene or audio is requested for this iteration. Panels/cards/status icons can be code-generated.

## Required four exports

Paths below are relative to `Project Love/Assets/_Project/Art/`.

| Character / work | File | Canvas | Pose/framing |
| --- | --- | --- | --- |
| Grace / Code | `Characters/Sprites/Mockup/Room/chr_grace_Seated_Working.png` | 1024 x 1024 | Local transparent seated working pose, hands suitable for the existing computer/keyboard direction; no desk, monitor or chair baked into the character |
| Grace / Code | `Characters/Sprites/Mockup/Dialogue/chr_grace_Normal.png` | 1500 x 2000 | Transparent waist/thigh-up VN framing comparable to Plaifah; neutral/attentive expression |
| Ploy / Design | `Characters/Sprites/Mockup/Room/chr_ploy_Seated_Working.png` | 1024 x 1024 | Local transparent seated planning/testing pose, holding a notebook/tablet; can fit the shared sofa/table area; no room furniture baked in |
| Ploy / Design | `Characters/Sprites/Mockup/Dialogue/chr_ploy_Normal.png` | 1500 x 2000 | Same VN framing/scale convention, neutral/attentive expression |

## Recommended four additional exports

| File | Canvas | Purpose |
| --- | --- | --- |
| `Characters/Sprites/Mockup/Room/chr_grace_Standing_Relaxed.png` | 1024 x 1024 | Relaxed standing pose for a later phase/break; no walk cycle required |
| `Characters/Sprites/Mockup/Dialogue/chr_grace_Smile.png` | 1500 x 2000 | Subtle warm expression; match her Normal canvas, scale and position exactly |
| `Characters/Sprites/Mockup/Room/chr_ploy_Standing_Relaxed.png` | 1024 x 1024 | Relaxed standing/idea-discussion pose, consistent projection |
| `Characters/Sprites/Mockup/Dialogue/chr_ploy_Smile.png` | 1500 x 2000 | Lively smile; same canvas, scale and position as her Normal |

If a seated break pose is easier for the chosen layout, agree its pose token before exporting rather than labeling a standing image Seated. Room pose tokens are independent of VN expression keys. `Normal`/`Smile` retain the existing CharacterExpression names.

## Identity and composition references

Character identity/appearance remain in the existing Thai profiles; this request does not rewrite or lock their full stories:

- [Grace, char_02_grace](../02_Narrative_And_World/Characters/03_Character_Profile_Grace_Programmer.md): dark hair, silver half-rim rectangular glasses, restrained smart office style. Use profile section 2 as the appearance source.
- [Ploy, char_03_ploy](../02_Narrative_And_World/Characters/04_Character_Profile_Ploy_Designer.md): warm caramel wavy hair, side ponytail option, bright casual style. Use profile section 2.
- Existing style/scale reference: [Plaifah room pose](../../Project%20Love/Assets/_Project/Art/Characters/Sprites/Mockup/Room/chr_plaifah_Seated_Working.png) and [Plaifah VN Normal](../../Project%20Love/Assets/_Project/Art/Characters/Sprites/Mockup/Dialogue/chr_plaifah_Normal.png).

The existing morning room has computer/laptop work areas plus shared sofa/table seating. Start with Grace near the computer and Ploy at a planning/rest seat; Plaifah can retain her tablet/art area. These are composition proposals, not fixed runtime coordinates. Do not require three full new desks or repaint all lighting variants before testing the cast.

Optional layout guide: save one annotated composite as `Art_Source/Mockup/ref_workspace_team_layout.png`, labeling each character/contact point. It need not be polished. Furniture differs across supplied phases, so integrate per-phase anchors rather than assuming identical seat coordinates.

## Export and placement

- PNG, RGBA, 8-bit/channel, sRGB, transparent background. Do not bake the black game backdrop, text, HUD or effects into the character export.
- Room poses are local sprites, not full-room layers. Keep a consistent scale/contact reference and avoid cropping useful body parts. The integration owns exact normalized placement/pivots. Foreground furniture can be handled separately if occlusion actually requires it; no foreground export is mandatory yet.
- Keep Normal/Smile aligned within each character's VN canvas to avoid expression-swap jumps. Do not force different body heights to identical anatomy; maintain comparable visual scale.
- Runtime root: `D:/Work/Unity/Project_Game/Visual Novel/Project Love/Assets/_Project/Art/` with the existing Room/Dialogue folders above. No new folder hierarchy is necessary.
- Editable masters may stay outside Assets in `Art_Source/Mockup/grace_master.psd` and `ploy_master.psd` (or the artist's preferred format).
- Use these exact ASCII names and existing token casing; do not append final/v2. Preserve any Unity-generated `.meta` beside a replacement image. Only new images should receive new GUIDs.

## Integration after delivery

Verify actual files/alpha/style/contact points, then bind Grace to Code and Ploy to Design through data. Add their independent daily talk/gift state and phase placements. Normal alone is sufficient; missing Smile falls back to Normal until supplied. That fallback and the two new cast bindings are **planned**, not already implemented.

The deferred dialogue status/work-preview requirement and sprint rules live in [Studio Work and Sprints](../01_Game_Design/03_Studio_Work_And_Sprints.md); this asset request does not authorize new runtime implementation before the current gameplay discussion is settled.
