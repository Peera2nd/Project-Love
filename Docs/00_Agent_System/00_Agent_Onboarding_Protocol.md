# 00 — Agent Onboarding and Continuity

Follow [AGENTS.md](../../AGENTS.md). This document supports continuity; it does not override owner instructions.

## Intake

Read in this exact order:

1. [Mistakes Registry](04_Mistakes_And_Prevention_Registry.md).
2. [Handoff](03_Handoff_And_Work_Logs.md): current snapshot and latest session.
3. [Architecture](01_Project_Context_And_Architecture.md).
4. [Master Decisions](../01_Game_Design/00_Master_Vision_And_Decisions.md).

Then read [Task Board](02_Task_Board_And_Roadmap.md) and the relevant specifications. The full campaign is approved at **30 days**; other content and balancing decisions retain their stated status.

## Language and source ownership

- English: backend design, technical specs, plans, tasks, handoffs, work logs, incidents, agent rules, and internal comments. Be concise and avoid duplicated explanations.
- Thai: user conversation and narrative/character documents in `Docs/02_Narrative_And_World/`, including their authoring templates. Existing English machine-facing prompts may remain; story dialogue stays Thai.
- Keep English identifiers and paths stable. Artist-facing tooltips/game text remain Thai-friendly.
- Sources: approval -> Master Decisions; implementation -> Task Board and files; character identity/speech -> profile sections 1–6; current wound -> section 5; schemas -> Data Architecture; expression keys -> Art Pipeline.
- Historical notes describe past state. Mark superseded assumptions explicitly.

## Execution

- Use generic, data-driven systems and designer-friendly editing tools. Keep content out of C#.
- **Text-bearing versions:** follow AGENTS rule 10 and the [Thai/English localization contract](../04_Technical_And_Tools/13_Localization_Architecture_And_Adoption_Contract.md). Session 064 implements L2 partial adoption, the actual translation tab and localized v9 saves; English remains incomplete. Preserve/extend this implementation for new/revised player text, update the adoption register and explicitly report adoption/remaining gaps to the owner in Thai. Do not recreate the foundation. Thai source and English game translations do not change the document-language policy. [Authoring guide](../04_Technical_And_Tools/14_Localization_Authoring_Guide.md).
- Follow AGENTS rule 8 / GR-012: routine content and UI need discoverable Thai-facing editing, persistent serialized sources, Undo/Save and safe previews. Actual Prefabs own layout; runtime binding must respect artist edits. Use the [current authoring workflow](../04_Technical_And_Tools/12_Artist_Workspace_And_UI_Authoring_Guide.md) as the acceptance baseline.
- Put owned assets under `_Project`; preserve meta/GUID pairs.
- Separate static definitions from serializable runtime/save state and runtime code from Editor code.
- Check related references when changing repeated information. Do not automatically approve unresolved content.
- Distinguish **Designed** (spec exists), **Implemented** (working code/assets exist), **Verified** (named check passed), and **Approved** (owner decision).
- Run checks appropriate to the change. For C# changes, verify compilation and relevant behavior. Documentation-only work requires reference/consistency checks, not an invented compile result.

## Incidents and closeout

Immediately log encountered failures, corrected misunderstandings, and fixed issues with their root cause (or an explicit unknown) and prevention rule.

Update Task Board, current Handoff snapshot, and add the newest English session above older history. Include completed work, changed files, verification and limits, issues, and next actions. Use [Work Log Template](WorkLogs/TEMPLATE_WorkLog.md) for substantial work. Send progress updates and the final reply in Thai.
