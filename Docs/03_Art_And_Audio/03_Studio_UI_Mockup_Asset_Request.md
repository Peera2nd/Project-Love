# Studio UI Mockup Asset Request

Session 031 · **Owner replacements integrated.** The initial 48-file package was generated in Session 028; the owner subsequently replaced icons/portraits and confirmed readiness. Current PNGs are preserved byte-for-byte with their existing GUIDs. 36 icons and three heads are bound, the canonical bar sheet is sliced into four shared layers, and four panel/frame assets are used. Optional individual bar exports remain available. Supplied colored icons retain their color. [Current controls/authoring](../04_Technical_And_Tools/04_Studio_UI_And_Dependency_Guide.md) · [runtime evidence](../00_Agent_System/WorkLogs/Session_031_Verification/README.md).

The specification below is the original request, not a reason to overwrite owner replacements. Backend brief: English; game labels remain Thai. Prototype integration does not constitute final production-art/usability approval.

## Visual direction and delivery

Friendly contemporary indie studio UI: simple rounded shapes, readable silhouettes, restrained detail, consistent stroke weight. Match the existing illustrated room/character mockups without requiring pixel art. Test icons at 32–48 displayed pixels; source images can be larger. No baked text, numbers, colored role backgrounds, character names, drop shadows outside the canvas, or button-state glow inside an icon.

- Icons: **256×256 RGBA PNG**, transparent, white/neutral grayscale. Center the visible shape in a 208×208 safe region (24 px margin). Keep stroke weight about 14–18 source pixels; avoid details narrower than 12 px. Unity's UI shader tints the shared grayscale sprites.
- Owner portraits: **256×256 RGBA PNG**, transparent square canvas, face/shoulders readable at 40–64 px. Supply the head cutout without a baked circle/frame. Reuse current character identity; frame and selection ring come from UI.
- Panels/cards/buttons/bars: grayscale RGBA, transparent corners. Keep 9-slice borders clean. Do not bake percent text or an already-filled progress value into a bar.
- Filenames: lowercase ASCII `ui_<purpose>.png`; owner avatars use `ui_avatar_<id>.png`. Keep existing PNGs/metas intact when replacing art. The agent handles Unity import/slicing; do not generate `.meta` files externally.
- Editable originals are optional under `Art_Source/Mockup/UI/`. Runtime exports belong under `Project Love/Assets/_Project/Art/UI/Mockup/` in the subfolders below. Folders may be delivered together; no atlas packing is required for individual icons.

## P0 — core icons

Place in **`Art/UI/Mockup/Icons/`**. One transparent PNG per row, all 256×256.

| Filename | Shape / use |
| --- | --- |
| `ui_heart.png` | Clear heart; affection |
| `ui_stamina.png` | Lightning bolt inside a simple energy silhouette; player energy |
| `ui_stress.png` | Tense face with one sweat mark; worker Stress, distinguish from energy |
| `ui_money.png` | Coin with a plain center; no currency letter |
| `ui_time.png` | Clock face with two hands; phase cost/time |
| `ui_bag.png` | Small satchel/backpack; inventory navigation |
| `ui_item.png` | Plain small box; generic item slot/category |
| `ui_sprint_board.png` | Three-column board with a few cards; Sprint navigation |
| `ui_rest.png` | Pillow with a small moon; worker/player rest, context supplied by label |
| `ui_overtime.png` | Moon and small clock; request/ordered OT |
| `ui_producer.png` | Clipboard with small gear; player's Producer work/progress |
| `ui_progress.png` | Short ascending bars with a check; overall project completion |
| `ui_code.png` | Angle-bracket pair; Code discipline |
| `ui_art.png` | Brush with broad paint tip; Art discipline |
| `ui_design.png` | Connected nodes or a small rule diagram; Design discipline |
| `ui_work.png` | Laptop; help/work action |
| `ui_talk.png` | Speech bubble; Talk action |
| `ui_gift.png` | Wrapped gift; Gift action |
| `ui_locked.png` | Closed padlock; Producer stage/dependency gate |
| `ui_blocked.png` | Warning triangle; actionable blocker |
| `ui_review.png` | Magnifier over a page; ready to review |
| `ui_done.png` | Strong check mark; accepted task |
| `ui_priority.png` | Up arrow above a card; prioritize |
| `ui_back.png` | Left/bent return arrow; modal back (existing vector arrow remains usable) |

The worker Stress icon intentionally differs from the Stamina bolt. Progress is also distinct from the sprint-board navigation icon. OT needs a visible state badge so players can tell who has already agreed without reopening every conversation.

## P0 — task owners

Place in **`Art/UI/Mockup/Portraits/`**:

| Filename | Subject |
| --- | --- |
| `ui_avatar_grace.png` | Grace, dark hair/rectangular glasses, head-and-shoulders |
| `ui_avatar_plaifah.png` | Plaifah, existing glasses/artist identity |
| `ui_avatar_ploy.png` | Ploy, caramel side ponytail, cheerful face |

Use existing Normal portraits as identity references. One expression each is enough. A separate Producer face is **not required**; use the clipboard badge until the player character design exists.

## P0 — one reusable progress-bar sheet

Place **`Art/UI/Mockup/Bars/ui_progress_bar_sheet.png`**:

- Canvas **1024×64**; four **256×64** cells in one horizontal row, left to right.
- Cell 0: empty dark track. Cell 1: grayscale solid fill. Cell 2: light border/frame with a transparent center. Cell 3: neutral diagonal forecast pattern on transparent background.
- All four use the same silhouette/alignment. Leave a 4 px outer transparent gutter inside each cell. Rounded ends should occupy no more than 16 px; suggested 9-slice border **16 px on all sides**.
- Track/fill/forecast/frame are separate layers in UI, never four different completion values. Runtime masks the fill horizontally and applies per-instance color through the shared UI shader. The fill/track must remain usable without the decorative frame.
- One sheet serves Code, Art, Design, Producer, Stamina, overall progress and Stress. **Do not export separate blue/purple/yellow bars.** Stress may use a reversed warning color; it is still the same geometry.

Optional: also export the four cells individually with suffixes `_track`, `_fill`, `_frame`, `_forecast` if convenient. The sheet is the canonical requested delivery, not both formats as extra mandatory work.

## P0 — cards and frames

Place in **`Art/UI/Mockup/Panels/`**:

| Filename | Canvas / slicing | Content |
| --- | --- | --- |
| `ui_task_card.png` | 512×320; 24 px 9-slice borders | Neutral rounded card background, quiet center for title/owner/bar, no role color or text |
| `ui_panel.png` | 512×512; 24 px borders | Reusable modal/section panel, readable dark/light contrast |
| `ui_avatar_frame.png` | 256×256; no slice | Transparent center, simple owner portrait outline |
| `ui_selection_ring.png` | 256×256; no slice | Transparent center, selected/OT/rest badge outline |

Cards use a colored discipline stripe/heading plus icon/text; never rely on color alone. Trial palette: **Code blue `#4EA6E8`, Art purple `#BB86E8`, Design amber `#E8B75A`, Producer teal `#56BEAA`**. Affection rose, Stamina green, Stress orange/red. Colors remain editable data, not baked art.

## P1 — useful follow-ups, not required to start

| Filename | Shape / use |
| --- | --- |
| `ui_travel.png` | Door with arrow; change scene |
| `ui_settings.png` | Gear; options |
| `ui_save.png` / `ui_load.png` | Save tray / tray with up arrow; explicit save/load |
| `ui_shop.png` | Shopping basket; store |
| `ui_bug.png` | Small software bug silhouette; added workload |
| `ui_dependency.png` | Two linked nodes; prerequisite |
| `ui_unlock.png` | Open padlock; newly available sprint |
| `ui_day.png` / `ui_night.png` | Sun / moon; phase state |
| `ui_close.png` | X; close panel |
| `ui_info.png` | Simple information marker; tooltip |

P1 icons follow the same 256-square specification. Button states can initially use Unity tint. If custom button art is desired later, request one grayscale 2×2 sheet (1024×256, 512×128 cells: normal/hover/pressed/disabled, 24 px borders).

## Integration and acceptance

The Session 031 integration replaces text-heavy presentation with bars, owner avatars, discipline colors, compact status icons and visible cost/OT/rest badges, while retaining numeric labels and tooltips for precision/accessibility.

Acceptance checks: correct filename/dimensions/RGBA; alpha corners/background; no clipped silhouette; consistent stroke/style; visible at 32 px on dark/light panels; correctly aligned sheet cells; clean 9-slice stretching; legible color-independent meaning. Verify in the real 1920×1080 game layout and a smaller window before approving production art.
