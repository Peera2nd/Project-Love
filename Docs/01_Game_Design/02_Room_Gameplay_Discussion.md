# Room Gameplay and Action Model — Discussion

Status: updated Session 022, 2026-10-24. The [isometric prototype](../04_Technical_And_Tools/02_Isometric_Prototype.md) now implements the supplied-art room/store view, phase placements, daily VN, time/Stamina, inventory gifts/reactions, contextual daily/repeat talk, attributed player work and save v3. Final balance, story and presentation remain iterative. Legacy AP fixtures remain supported.

## Owner direction

- Clarify the full player experience before implementing a minigame or requesting art assets.
- Session 017 clarifies fixed isometric action scenes with money/time/Stamina/inventory/options/travel HUD. Click people/objects for contextual actions; dropped items may be picked up, and shop-clerk interaction opens purchases. Owner supplies art.
- Session 022: ordinary conversation is always repeatable. The first completed talk per character/day uses a little Stamina and gives a little affection; subsequent talk is free without rewards. No phase cost. Daily reward gates refresh next day, not access.
- Completed work and rest must each advance one time phase.
- Session 015 one-phase ordinary talk is superseded by Session 022. Scripted story events retain independently authored duration; ordinary talk uses the separate interaction/reward policy.
- Room config now uses data-authored Stamina trial values. Small-help rules, final resource costs/recovery and overall feasibility remain open; this implementation is not validated balance.
- Full campaign remains 30 days; no walking exploration. Isometric scene framing is now directed; exact projection/composition, final setting and roster remain open.

## Previously confirmed prototype gaps — resolved Session 019

ERR-020 daily access is fixed by separate daily/permanent completion records. Work/rest consume authored phases; the Session 019 ordinary-talk phase charge was replaced by the Session 022 zero-phase social policy. Deferral restores the pre-conversation state without time/resource/reward loss. Completing the last phase ends the day; next-day input is explicit. Earlier passing tests covered only the old fixtures; current evidence is in the room guide.

## Proposed player-facing structure

The workspace is the hub. Keep date/phase, player energy and key objective information compact. Characters and interactive room objects form the primary action entry points.

- Character click immediately opens the large VN portrait/greeting with Work/Talk/Gift choices. Tap the scene to reveal/advance text; bottom-right arrow returns/cancels. Availability and costs are visible before committing. Implemented in Session 022.
- Player desk/computer opens personal work. A rest object offers recovery. A door/destination menu can support shopping later without a walking map.
- Conversation dims the current scene and overlays a larger character plus dialogue/choices, then returns to the room. Room clicks must be blocked while this modal is open. A work activity uses a focused panel or screen, then returns with visible results.
- Confirm the complete cost before starting. Commit resource effects and elapsed time together once on completion. Cancelled selection/deferred dialogue does not advance time; final-phase rollover and special-event duration still need explicit rules.
- Character pose and placement change by phase: e.g. morning desk work to afternoon phone use on a sofa. Use data-authored anchors/schedules and separate occluders, with direct state swaps sufficient initially. Lighting/mood feedback remains useful; walking animation is not required.

## Time and energy model — presentation directed, exact rules open

Owner specifies time and player Stamina in the intended HUD. The room uses configurable trial costs/capacity/recovery; legacy AP configs retain their mode. Save v3 records resource mode and social/work state; v2 migrates without invented work attribution, and v1 remains AP-only. Keep player Stamina distinct from character Stress.

| Interaction | Proposed phase cost | Proposed energy rule |
| --- | --- | --- |
| Read status, inspect a task, open/close a menu | None | None |
| Character greeting/menu | None | Free; no reward |
| Ordinary repeatable talk | None, owner decision Session 022 | First completion per character/day: trial -3, affection +2; later repeats free/no reward |
| Scripted story event | Independently authored | Independently authored; do not use repeatable talk to run story effects |
| Work | One phase, implemented | Trial -20, adjustable by task |
| Minor help | No full phase, proposed | Example -10; bounded opportunities/rewards |
| Rest | One phase, implemented | Trial +30 energy; final amount open |
| Shopping trip, later | One phase, proposed | Travel effort open; browsing/purchasing is not charged another phase per click/item |

The distinction between minor help and a substantial activity needs owner agreement. Phase count, all energy values, recovery/carryover and daily reward limits remain open. A 100-point meter alone does not add strategic depth: with four opportunities costing 20 each, full daily energy never binds. Workload variation or recovery rules need testing rather than assuming the larger number fixes balance.

## Repeatable interaction versus narrative event

Daily conversation access and one-time story progression are separate rules. Ordinary talk access remains repeatable; its reward/cost gate refreshes by day. Important scenes remain once-only unless explicitly authored otherwise. Access, content selection and reward frequency must be tracked separately, so daily talk does not replay an introduction or farm affection indefinitely. Reusable short text variants can carry early prototypes; do not promise 30 unique scenes per character.

## Next discussion and milestone

Implement one small sprint/task board with per-discipline minimums, explicit passive-team work timing and review/carryover. Owner confirms that the team works independently and the player helps bottlenecks. See [Studio Work and Sprints](03_Studio_Work_And_Sprints.md) for proposed rules, open costs and current implementation boundaries. Tune presentation from feedback before claiming a complete balance model. Navigation, pickup, purchases and recovery-item use currently cost no phase; UI makes these prototype rules explicit. Specific minigame rules remain later work.

## Balance review checkpoint — Session 015, revised Session 022

Keep conversation duration in activity/interaction authoring data so later tuning does not require rewriting dialogue flow. Session 022 ordinary talk uses no phase and first-daily energy only; scripted story-event duration stays separate. The original one-phase rule is historical.

Once the interacting systems are usable, evaluate 30-day schedules against required work output, energy consumption/recovery, relationship gains/gates and event windows. Include work-focused, relationship-focused and ordinary-mistake/recovery schedules; determine feasibility for intended paths without assuming every relationship must be maximized in one run. Account for mandatory events and recovery time. Story/action duration, first-daily talk energy/rewards, output/growth rates and requirements remain tuning variables; feasibility has not been established.

## Artist delivery handoff — Session 016

Historical Session 016 static-layer request is superseded by the updated [isometric brief](../03_Art_And_Audio/01_Workspace_Mockup_Asset_Brief.md). Session 017 provides two visual references and requests a small overview package: local character poses, phase placement, scene occluders, dialogue overlay and a second shop scene/item. References are not runtime art. Code can use placeholders; that historical pending state is superseded by Session 019 implementation.

Session 019 uses the supplied 1024x1024 room variants over black. Room Phone artwork is upright and fine seating/foreground occlusion remain mockup polish. Five workspace images do not create five action phases; LateNight is the ended-day visual.
