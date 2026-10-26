# Workspace mockup source art

Optional editable PSD/PSB/KRA files and annotated layout references belong here, outside Unity Assets.

Runtime delivery is separate transparent/opaque PNG exports under `Project Love/Assets/_Project/Art/`.
See the canonical [workspace mockup asset brief](../../Docs/03_Art_And_Audio/01_Workspace_Mockup_Asset_Brief.md) for exact paths, dimensions, naming and priorities.

Suggested source filenames: `workspace_master.psd`, `plaifah_master.psd`, `ref_workspace_layout.png`.
Source revisions may be versioned here; keep runtime export names stable.

Session 017 references are retained in `References/ref_isometric_room.png` and `References/ref_dialogue_overlay.png`. They illustrate scene framing and modal interaction, not automatic runtime-art reuse or a required pixel-art style. Annotated layout composites may identify desk/sofa/clerk anchors. Runtime room characters now use local pose cutouts; the earlier full-room fixed-character export default is superseded.

Session 024: Delivered all eight recommended Grace and Ploy mockup PNGs to `Project Love/Assets/_Project/Art/Characters/Sprites/Mockup/` (Room and Dialogue subfolders) generated via local SD WebUI and rembg. Raw outputs are stored in `SD_Generations/` and annotated team placement guide is saved in `ref_workspace_team_layout.png`. Integration into Unity RoomDataSO remains planned.

Session 028: Delivered 48 Studio UI Mockup PNGs to `Project Love/Assets/_Project/Art/UI/Mockup/` (Icons, Portraits, Bars, Panels) generated via local SD WebUI and PIL. Raw icon generations are preserved in `SD_Generations/UI/` and UI overview composite is saved in `UI/ref_ui_mockup_overview.png`. Generator script is in `generate_ui_mockups.py`.
