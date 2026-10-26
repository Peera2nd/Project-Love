using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectLove.UI
{
    public sealed class IsometricRoomView : MonoBehaviour
    {
        public GameConfigSO config;
        public RoomPresentationSO presentation;
        public Image roomImage, dialoguePortrait;
        public RectTransform hotspotRoot;
        public TMP_Text hudText, titleText, hintText, panelTitle, panelText, speakerText, dialogueText;
        public Transform panelButtons, choiceButtons;
        public GameObject dialogueModal;
        public Button buttonTemplate, continueButton, deferButton, travelButton, inventoryButton, optionsButton, waitButton, nextDayButton;
        public GameSession Session { get; private set; }
        public AuthoredRomanceLayout Authored => GetComponent<AuthoredRomanceLayout>();
        public bool IsEditorPreview { get; private set; }
        public SaveStore Saves { get; private set; }
        public Dictionary<string, Button> HotspotButtons { get; } = new Dictionary<string, Button>();
        public Dictionary<string, Button> EventBubbleButtons { get; } = new Dictionary<string, Button>();
        public bool IsRevealing { get; private set; }
        public Button DialogueTapButton { get; private set; }
        public float TextSpeed { get; set; }
        public string T(string key) => Session!=null?Session.Localization.Presentation(presentation,key):presentation.Get(key);
        public string Name(IdentifiedDataSO data) => data==null?"":Session.Localization.Content(data,"displayName", data is CharacterDataSO c?c.displayName:data is RoomDataSO r?r.displayName:data is ItemDataSO i?i.displayName:data is EventDataSO e?e.displayName:data is ActivityDataSO a?a.displayName:"");
        public string Phase(DayPhase phase)=>Session.Localization.Content(presentation,"phase/"+phase,presentation.phaseNames[(int)phase]);
        private readonly Dictionary<TMP_Text,TMP_FontAsset> authoredFonts=new Dictionary<TMP_Text,TMP_FontAsset>();
        private void ApplyLocaleFonts(){foreach(var t in GetComponentsInChildren<TMP_Text>(true)){if(!authoredFonts.ContainsKey(t))authoredFonts[t]=t.font;var font=Session.Localization.Font??authoredFonts[t];if(t.font!=font)t.font=font;}}
        private void OnLanguageChanged(){StopReveal();displayedLine=null;notice="";Refresh();RevealAll();if(Authored!=null){if(Authored.focusRoot.gameObject.activeSelf)Authored.BindFocus();if(Authored.recapRoot.gameObject.activeSelf)Authored.BindReport(Session.Romance.PendingReport);}}
        public void SetLanguage(string locale)=>Session.Localization.SetLocale(locale,!IsEditorPreview);
        private enum PanelMode { Overview, Context, Travel, Inventory, Options, Shop }
        private PanelMode panelMode;
        public bool IsOverviewPanel => panelMode == PanelMode.Overview;
        public string CurrentNotice => notice;
        private RoomHotspot selected;
        private Coroutine reveal;
        private string displayedLine, notice = "";
        private int lastDay;
        private bool lastEnded, restoring, choosingGift;
        private bool autoSavePending;

        private void LateUpdate()
        {
            if (Session == null || restoring) return;
            // UI pumping happens after transactions/notifications, never recursively inside a command.
            if (Session.Events.TryBeginForced()) return;
            if (autoSavePending && !Session.IsBusy)
            {
                autoSavePending = false;
                notice = Saves.Save(Session, SaveSlot.Auto) == SaveResult.Success ? T("autoSaved") : T("saveFailed");
                Refresh();
            }
        }

        private void Start()
        {
            if (config.production != null && GetComponent<StudioSprintView>() == null) gameObject.AddComponent<StudioSprintView>();
            if (config.romance != null && GetComponent<RomanceRoomView>() == null) gameObject.AddComponent<RomanceRoomView>();
            TextSpeed = presentation.charactersPerSecond;
            Saves = new SaveStore(Path.Combine(Application.persistentDataPath, "PrototypeSaves"));
            continueButton.gameObject.SetActive(false);
            DialogueTapButton = dialogueModal.GetComponent<Button>();
            if (DialogueTapButton == null) DialogueTapButton = dialogueModal.AddComponent<Button>();
            DialogueTapButton.transition = Selectable.Transition.None;
            DialogueTapButton.onClick.AddListener(Continue);
            deferButton.onClick.AddListener(Back);
            if(Authored == null)
            {
            var backRect = (RectTransform)deferButton.transform;
            backRect.anchorMin = new Vector2(.92f, .03f); backRect.anchorMax = new Vector2(.98f, .11f);
            backRect.offsetMin = backRect.offsetMax = Vector2.zero;
            deferButton.GetComponentInChildren<TMP_Text>(true).gameObject.SetActive(false);
            var arrow = new GameObject("Back Arrow", typeof(RectTransform), typeof(CanvasRenderer), typeof(BackArrowGraphic));
            var arrowRect = arrow.GetComponent<RectTransform>(); arrowRect.SetParent(backRect, false);
            arrowRect.anchorMin = new Vector2(.1f, .1f); arrowRect.anchorMax = new Vector2(.9f, .9f);
            arrowRect.offsetMin = arrowRect.offsetMax = Vector2.zero;
            arrow.GetComponent<BackArrowGraphic>().raycastTarget = false;
            MakeScrollableChoices();
            }
            travelButton.onClick.AddListener(() => ShowPanel(PanelMode.Travel));
            inventoryButton.onClick.AddListener(() => ShowPanel(PanelMode.Inventory));
            optionsButton.onClick.AddListener(() => ShowPanel(PanelMode.Options));
            waitButton.onClick.AddListener(() => { if (!Session.IsBusy) Session.TryEndDay(); });
            nextDayButton.onClick.AddListener(() => Session.TryStartNextDay());
            Restart();
        }
        private void OnDestroy() { if (Session != null) {Session.Changed -= OnChanged;Session.Localization.Changed-=OnLanguageChanged;} }
        public void InitializeEditorPreview(GameSession session)
        {
            IsEditorPreview=true;Session=session;Session.Localization.Changed+=OnLanguageChanged;TextSpeed=0;displayedLine=null;panelMode=PanelMode.Overview;
            DialogueTapButton=dialogueModal.GetComponent<Button>();Refresh();
        }
        public void SetSaveDirectory(string path) { Saves = new SaveStore(path); }
        public void Restart()
        {
            if (Session != null) {Session.Changed -= OnChanged;Session.Localization.Changed-=OnLanguageChanged;}
            StopReveal(); displayedLine = null;
            Session = new GameSession(config);Session.Localization.LoadPreference();Session.Localization.Changed+=OnLanguageChanged;
            lastDay = Session.State.day; lastEnded = false;
            Session.Changed += OnChanged;
            panelMode = PanelMode.Overview; selected = null; choosingGift = false; notice = "";
            autoSavePending = false;
            Refresh();
        }
        private void OnChanged()
        {
            var state = Session.State;
            if (!restoring && (state.day != lastDay || (state.dayEnded && !lastEnded)))
                autoSavePending = true;
            lastDay = state.day; lastEnded = state.dayEnded;
            if (selected != null && !Session.CurrentRoom.hotspots.Contains(selected)) { selected = null; panelMode = PanelMode.Overview; }
            Refresh();
        }
        public SaveResult Save(SaveSlot slot)
        {
            var result = Saves.Save(Session, slot);
            notice = result == SaveResult.Success ? T("saved") : T("saveFailed");
            Refresh(); return result;
        }
        public SaveResult Load(SaveSlot slot)
        {
            restoring = true;
            SaveResult result;
            try { result = Saves.Load(Session, slot); }
            finally { restoring = false; }
            lastDay = Session.State.day; lastEnded = Session.State.dayEnded;
            panelMode = PanelMode.Overview; selected = null;
            if (result == SaveResult.Success) autoSavePending = false;
            notice = result == SaveResult.Success ? T("loaded") : T("loadFailed");
            Refresh(); return result;
        }
        public void ClosePanel() => ShowPanel(PanelMode.Overview);
        public void OpenGifts() { choosingGift = true; Refresh(); }
        public RoomHotspot RestHotspot => Session.CurrentRoom.hotspots.FirstOrDefault(h=>h.kind==RoomInteractionKind.Activity && h.activity!=null && h.activity.staminaRecovery>0 && h.placements.Any(p=>p.visible&&p.phase==Session.State.phase));
        public void OpenRestMenu(){if(RestHotspot!=null)SelectHotspot(RestHotspot);}
        private void CompleteContextActivity(ActivityDataSO activity)
        {
            if(!Session.Production.PlanningEnabled){Session.TryCompleteActivity(activity,true);return;}
            var previous=selected;panelMode=PanelMode.Overview;selected=null;notice="";
            if(!Session.TryCompleteActivity(activity,true)){selected=previous;panelMode=PanelMode.Context;Refresh();}
        }
        private void ShowPanel(PanelMode mode)
        {
            if (Session.IsBusy) return;
            selected = null; panelMode = mode; notice = ""; Refresh();
        }
        public void SelectHotspot(RoomHotspot hotspot)
        {
            if (Session.IsBusy || Session.State.dayEnded || hotspot == null || !Session.CurrentRoom.hotspots.Contains(hotspot)) return;
            var placement = hotspot.placements.FirstOrDefault(p => p.phase == Session.State.phase);
            if (placement == null || !placement.visible) return;
            if (hotspot.kind == RoomInteractionKind.Pickup) { Session.TryCollect(hotspot); return; }
            if (hotspot.kind == RoomInteractionKind.Character) { choosingGift = false; Session.Interactions.TryOpen(hotspot); return; }
            if (hotspot.kind == RoomInteractionKind.Producer) { GetComponent<StudioSprintView>()?.ShowBoard(); return; }
            selected = hotspot;
            panelMode = hotspot.kind == RoomInteractionKind.Shop ? PanelMode.Shop : PanelMode.Context;
            notice = ""; Refresh();
        }
        public void Continue()
        {
            if (IsRevealing) { RevealAll(); return; }
            if (!Session.Dialogue.IsActive) return;
            if (!Session.Dialogue.TryAdvance() && !Session.Dialogue.ChoicesReady) Session.Dialogue.TryDefer();
        }
        private void Back()
        {
            if (Session.Dialogue.IsActive) Session.Dialogue.TryDefer();
            else if (choosingGift) { choosingGift = false; Refresh(); }
            else Session.Interactions.TryClose();
        }
        public void RevealAll()
        {
            StopReveal(); dialogueText.maxVisibleCharacters = int.MaxValue;
        }
        private void StopReveal()
        {
            if (reveal != null) StopCoroutine(reveal);
            reveal = null; IsRevealing = false;
        }
        private string Cost(int phases, int stamina) => LocalizationService.Format(T("cost"), phases, stamina);
        public void Refresh()
        {
            var state = Session.State;
            var room = Session.CurrentRoom;
            hudText.text = LocalizationService.Format(T("hud"), state.day, config.totalDays,
                state.dayEnded ? T("dayEnded") : Phase(state.phase),
                state.money, state.stamina, config.maxStamina, state.mainProgress);
            titleText.text = Name(room);
            hintText.text = state.dayEnded ? (Session.Days.CampaignEnded ? T("campaignEnded") : T("summary")) : T("hint");
            roomImage.sprite = room.Background(state.phase, state.dayEnded);
            roomImage.preserveAspect = true;
            RenderHotspots(room);
            bool active = Session.IsBusy;
            travelButton.interactable = !active && !state.dayEnded;
            inventoryButton.interactable = !active;
            optionsButton.interactable = !active;
            waitButton.interactable = !active && !state.dayEnded;
            nextDayButton.gameObject.SetActive(state.dayEnded && !Session.Days.CampaignEnded);
            Label(travelButton, T("travel")); Label(inventoryButton, T("inventory"));
            Label(optionsButton, T("options")); Label(waitButton, T("endDay"));
            Label(nextDayButton, T("nextDay"));ApplyLocaleFonts();
            RenderPanel(); RenderDialogue(); GetComponent<StudioSprintView>()?.Refresh(); GetComponent<RomanceRoomView>()?.Refresh();
            if (presentation.useStudioSkin)
            {
                StudioUiWidgets.DecorateButton(travelButton, presentation, presentation.Icon("travel"));
                StudioUiWidgets.DecorateButton(inventoryButton, presentation, presentation.Icon("bag"));
                StudioUiWidgets.DecorateButton(optionsButton, presentation, presentation.Icon("settings"));
                StudioUiWidgets.DecorateButton(waitButton, presentation, presentation.Icon("night"));
                StudioUiWidgets.DecorateButton(nextDayButton, presentation, presentation.Icon("day"));
            }
        }
        private void RenderHotspots(RoomDataSO room)
        {
            if(Authored!=null){Authored.RenderRoom(Session);return;}
            Clear(hotspotRoot); HotspotButtons.Clear(); EventBubbleButtons.Clear();
            foreach (var hotspot in room.hotspots)
            {
                if((Session.Production.PlanningEnabled || Session.Romance.Enabled) && (hotspot.kind==RoomInteractionKind.Producer || hotspot.kind==RoomInteractionKind.Activity&&hotspot.activity!=null&&hotspot.activity.staminaRecovery>0))continue;
                if (hotspot.kind == RoomInteractionKind.Pickup && Session.State.collectedPickups.Contains(hotspot.id)) continue;
                var place = hotspot.placements.FirstOrDefault(p => p.phase == Session.State.phase);
                if (place == null || !place.visible || Session.State.dayEnded) continue;
                var captured = hotspot;
                var go = new GameObject(hotspot.id, typeof(RectTransform), typeof(Image), typeof(Button));
                var rect = go.GetComponent<RectTransform>(); rect.SetParent(hotspotRoot, false);
                rect.anchorMin = place.position - place.size / 2; rect.anchorMax = place.position + place.size / 2;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                var image = go.GetComponent<Image>(); image.sprite = place.sprite;
                image.color = place.sprite != null ? Color.white : new Color(.2f, .4f, .45f, .28f);
                image.preserveAspect = true;
                if (place.sprite != null) image.alphaHitTestMinimumThreshold = .1f;
                var button = go.GetComponent<Button>(); button.targetGraphic = image;
                button.interactable = !Session.IsBusy;
                button.onClick.AddListener(() => SelectHotspot(captured));
                HotspotButtons.Add(hotspot.id, button);
                // Label only non-character world objects; character art remains the primary click target.
                if (place.sprite == null)
                {
                    var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                    var lr = label.GetComponent<RectTransform>(); lr.SetParent(rect, false);
                    lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;
                    var text = label.GetComponent<TextMeshProUGUI>(); text.font = presentation.font; text.text = hotspot.label;
                    text.fontSize = 24; text.enableAutoSizing = true; text.fontSizeMin = 18; text.fontSizeMax = 24;
                    text.color = Color.white; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
                }
            }
            foreach (var data in Session.Events.BubbleEvents())
            {
                var hotspot = room.hotspots.First(h => h.character == data.character && h.placements.Any(p => p.phase == Session.State.phase && p.visible));
                var place = hotspot.placements.First(p => p.phase == Session.State.phase);
                var button = Instantiate(buttonTemplate, hotspotRoot); button.gameObject.SetActive(true);
                button.name = "Event Bubble " + data.id;
                var rect = (RectTransform)button.transform;
                float y = Mathf.Clamp(place.position.y + place.size.y * .40f, .08f, .92f);
                float x = Mathf.Clamp(place.position.x, .08f, .92f);
                rect.anchorMin = new Vector2(x - .055f, y - .035f); rect.anchorMax = new Vector2(x + .055f, y + .035f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                Label(button, T("eventBubble"));
                button.onClick.AddListener(() => Session.Dialogue.TryBegin(data));
                EventBubbleButtons.Add(data.id, button);
                if (presentation.useStudioSkin)
                {
                    StudioUiWidgets.DecorateButton(button, presentation, presentation.Icon("talk"),
                        LocalizationService.Format(T("eventHint"), Name(data), data.startDay, data.endDay, data.elapsedPhases, data.staminaCost));
                }
            }
            if (presentation.useStudioSkin && Session.Production.Enabled) RenderOrderBadges(room);
        }
        private void RenderOrderBadges(RoomDataSO room)
        {
            foreach (var order in Session.State.production.orders)
            {
                var hotspot = room.hotspots.FirstOrDefault(h => h.character != null && h.character.id == order.characterId);
                var place = hotspot?.placements.FirstOrDefault(p => p.phase == Session.State.phase && p.visible);
                if (place == null || Session.State.dayEnded) continue;
                bool overtime = order.kind == StaffOrderKind.Overtime;
                var center = place.position + new Vector2(place.size.x * .32f, -.04f);
                var icon = StudioUiWidgets.Image("Order Badge " + order.characterId, hotspotRoot, center.x - .025f, center.y - .025f,
                    center.x + .025f, center.y + .025f, presentation.Icon(overtime ? "overtime" : "rest"), Color.white);
                icon.preserveAspect = true;
                StudioUiWidgets.Image("Order Ring", icon.transform, -.12f, -.12f, 1.12f, 1.12f, presentation.selectionRingSprite,
                    overtime ? presentation.stressTint : presentation.staminaTint).preserveAspect = true;
                StudioUiWidgets.Hint(icon.gameObject, presentation, LocalizationService.Format(T(overtime ? "otBadgeHint" : "restBadgeHint"), hotspot.character.displayName));
            }
        }
        private void RenderPanel()
        {
            Clear(panelButtons);
            if(Authored==null)
            {
            panelText.rectTransform.anchorMin = new Vector2(.05f, .61f);
            panelText.rectTransform.anchorMax = new Vector2(.95f, .85f);
            panelText.fontSizeMin = 18; panelText.fontSizeMax = 25;
            }
            panelTitle.text = T("overviewTitle");
            panelText.text = notice.Length > 0 ? notice : T("overview");
            if (panelMode == PanelMode.Overview && notice.Length == 0 && Session.Production.Enabled)
            {
                panelText.text = GetComponent<StudioSprintView>()?.SprintSummary() ?? T("overview");
                panelText.rectTransform.anchorMin = new Vector2(.05f, .32f);
                if (!string.IsNullOrEmpty(Session.State.production.lastReport)) panelText.text += "\n\n" + Session.State.production.lastReport;
            }
            else if (panelMode == PanelMode.Overview && notice.Length == 0 && config.workStreams.Length > 0)
                panelText.text += "\n" + string.Join("\n", config.workStreams.Select(s => LocalizationService.Format(T("workStream"), s.displayName, Session.WorkCompleted(s))));
            if (Session.State.dayEnded && panelMode != PanelMode.Options && panelMode != PanelMode.Inventory) return;
            switch (panelMode)
            {
                case PanelMode.Context:
                    if (selected == null) break;
                    panelTitle.text = selected.label;
                    if (selected.character != null)
                    {
                        var person = Session.State.FindCharacter(selected.character.id);
                        panelText.text = LocalizationService.Format(T("characterStats"), person.affection, person.stress);
                        var data = selected.conversation;
                        if (data != null) AddButton(panelButtons, T("talk") + "\n" + Cost(data.elapsedPhases, data.staminaCost),
                            () => Session.Dialogue.TryBegin(data), Session.CanStartEvent(data));
                    }
                    if (selected.activity != null)
                    {
                        var data = selected.activity;
                        panelText.text = Cost(data.elapsedPhases, data.staminaCost);
                        AddButton(panelButtons, Name(data), () => CompleteContextActivity(data), Session.CanCompleteActivity(data, true));
                        if(Session.Production.PlanningEnabled)
                        {
                            panelText.text="";
                            PlanningActionRow.Render(panelButtons.GetComponentsInChildren<Button>().Last(),presentation,Name(data),
                                new ActionMetric("stamina",data.staminaRecovery-data.staminaCost),new ActionMetric("time",data.elapsedPhases));
                        }
                    }
                    break;
                case PanelMode.Travel:
                    panelTitle.text = T("travel"); panelText.text = T("travelHint");
                    foreach (var room in config.rooms)
                    {
                        var data = room;
                        AddButton(panelButtons, Name(room), () => { panelMode = PanelMode.Overview; Session.TryTravel(data); }, Session.CanTravel(data),
                            presentation.Icon(room.shopItems.Length > 0 ? "shop" : "travel"));
                    }
                    break;
                case PanelMode.Inventory:
                    panelTitle.text = T("inventory"); panelText.text = notice.Length > 0 ? notice : T("inventoryHint");
                    foreach (var item in config.items)
                    {
                        int count = Session.ItemCount(item); if (count == 0) continue;
                        var data = item;
                        AddButton(panelButtons, LocalizationService.Format(T("itemUse"), Name(item), count),
                            () => Session.TryUse(data), Session.CanUse(item), item.icon != null ? item.icon : presentation.Icon("item"));
                    }
                    break;
                case PanelMode.Shop:
                    panelTitle.text = T("shop"); panelText.text = T("shopHint");
                    foreach (var item in Session.CurrentRoom.shopItems)
                    {
                        var data = item;
                        AddButton(panelButtons, LocalizationService.Format(T("itemBuy"), Name(item), item.price),
                            () => Session.TryBuy(data), Session.CanBuy(item), item.icon != null ? item.icon : presentation.Icon("item"));
                    }
                    break;
                case PanelMode.Options:
                    panelTitle.text = T("options"); panelText.text = notice.Length > 0 ? notice : T("optionsHint");
                    AddButton(panelButtons, T("save"), () => Save(SaveSlot.Manual), !Session.Dialogue.IsActive, presentation.Icon("save"));
                    AddButton(panelButtons, T("load"), () => Load(SaveSlot.Manual), !Session.Dialogue.IsActive && Saves.Exists(Session, SaveSlot.Manual), presentation.Icon("load"));
                    AddButton(panelButtons, T("loadAuto"), () => Load(SaveSlot.Auto), !Session.Dialogue.IsActive && Saves.Exists(Session, SaveSlot.Auto), presentation.Icon("load"));
                    AddButton(panelButtons, T("restart"), Restart, !Session.Dialogue.IsActive);
                    if(Session.Localization.Enabled){AddButton(panelButtons,T("languageThai"),()=>SetLanguage("th"),true);AddButton(panelButtons,T("languageEnglish"),()=>SetLanguage("en"),true);}
                    break;
            }
            if (panelMode == PanelMode.Overview)
                foreach (var data in Session.Events.DeferredGlobalEvents())
                {
                    var captured = data;
                    AddButton(panelButtons, Name(data), () => Session.Dialogue.TryBegin(captured), Session.CanStartEvent(data));
                }
        }
        private void RenderDialogue()
        {
            var runner = Session.Dialogue;
            bool active = Session.Dialogue.IsActive || Session.Interactions.IsOpen;
            dialogueModal.SetActive(active);
            Clear(choiceButtons);
            if (!active) { StopReveal(); displayedLine = null; return; }
            continueButton.gameObject.SetActive(false);
            DialogueTapButton.interactable = true;
            if (!runner.IsActive) { RenderCharacterMenu(); return; }
            var line = runner.CurrentLine ?? (runner.ChoicesReady ? runner.Node.lines.LastOrDefault() : null);
            if (line != null && displayedLine != line.lineId)
            {
                StopReveal(); displayedLine = line.lineId;
                speakerText.text = Name(line.speaker);
                dialoguePortrait.sprite = line.speaker != null ? line.speaker.GetSprite(line.expression) : null;
                dialoguePortrait.enabled = dialoguePortrait.sprite != null;
                dialogueText.text = Session.Localization.Line(runner.Node,line); dialogueText.maxVisibleCharacters = 0; dialogueText.ForceMeshUpdate();
                BeginReveal();
            }
            if (runner.ChoicesReady)
            {
                RevealAll();
                foreach (var choice in runner.Node.choices)
                {
                    if (choice == null) continue;
                    var captured = choice;
                    var button = AddButton(choiceButtons, Session.Production.Enabled ? Session.Production.DialogueWorkLabel(choice) : Session.Localization.Choice(runner.Node,choice),
                        () => runner.TryChoose(captured), runner.CanChoose(choice));
                    if (Session.Romance.Enabled)
                    {
                        int affection=choice.effects.Where(e=>e.stat==StatType.Affection).Sum(e=>Math.Min(e.amount,100-Session.State.FindCharacter(e.character.id).affection));
                        PlanningActionRow.Render(button,presentation,Session.Localization.Choice(runner.Node,choice),new ActionMetric("heart",affection),new ActionMetric("time",-(runner.ActiveEvent?.elapsedPhases??0)));
                    }
                    if (choice.workAction != DialogueWorkAction.None)
                    {
                        button.GetComponent<LayoutElement>().preferredHeight = 116;
                        if(Session.Production.PlanningEnabled)
                        {
                            var f=Session.Production.PreviewDialogueWork(choice);
                            PlanningActionRow.Render(button,presentation,Session.Localization.Choice(runner.Node,choice),
                                new ActionMetric("money",-f.moneyCost),new ActionMetric("stamina",-f.staminaCost),
                                new ActionMetric("time",choice.workAction==DialogueWorkAction.WorkTogether?1:0),
                                new ActionMetric("stress",f.stressAfter-f.stressBefore),new ActionMetric("heart",f.affectionGain));
                        }
                    }
                }
                if (!runner.HasAvailableChoice) dialogueText.text = T("blocked");
            }
        }
        private void RenderCharacterMenu()
        {
            var interaction = Session.Interactions;
            var character = interaction.Character;
            var greeting = interaction.Greeting?.lines.FirstOrDefault();
            string key = "menu:" + character.id + ":" + (greeting?.lineId ?? "") + ":" + choosingGift;
            if (displayedLine != key)
            {
                StopReveal(); displayedLine = key;
                speakerText.text = Name(character);
                dialoguePortrait.sprite = character.GetSprite(greeting?.expression ?? CharacterExpression.Normal);
                dialoguePortrait.enabled = dialoguePortrait.sprite != null;
                dialogueText.text = choosingGift ? T("giftHint") : (greeting!=null?Session.Localization.Line(interaction.Greeting,greeting):T("greeting"));
                dialogueText.maxVisibleCharacters = 0; dialogueText.ForceMeshUpdate();
                BeginReveal();
            }
            if(Session.Romance.Enabled&&!choosingGift){GetComponent<RomanceRoomView>()?.RenderCharacterMenu(character);return;}
            if(Session.Production.PlanningEnabled&&!choosingGift){RenderPlanningCharacterMenu(character);return;}
            if (choosingGift)
            {
                var items = config.items.Where(i => i.giftable && Session.ItemCount(i) > 0).ToArray();
                if (items.Length == 0) { RevealAll(); dialogueText.text = T("noGifts"); }
                foreach (var item in items)
                {
                    var data = item;
                    AddButton(choiceButtons, LocalizationService.Format(T("giftItem"), Name(data), Session.ItemCount(data)),
                        () => { choosingGift = false; interaction.TryGift(data); }, interaction.CanGift(data), data.icon);
                }
                return;
            }
            string talkLabel = interaction.RewardAvailable ? LocalizationService.Format(T("dailyTalk"), interaction.TalkCost, character.dailyTalkAffection)
                : T("repeatTalk");
            AddButton(choiceButtons, Session.Production.Enabled ? GetComponent<StudioSprintView>().HelpLabel(character) :
                T("workTogether") + "\n" + (character.workActivity != null ?
                Cost(character.workActivity.elapsedPhases, character.workActivity.staminaCost) : T("unavailableWork")),
                () => interaction.TryWork(), interaction.CanWork, presentation.Icon("work"));
            AddButton(choiceButtons, talkLabel, () => interaction.TryTalk(), interaction.CanTalk, presentation.Icon("talk"));
            AddButton(choiceButtons, T("gift"), () => { choosingGift = true; Refresh(); }, true, presentation.Icon("gift"));
            if (Session.Production.Enabled)
            {
                var kind = Session.State.phase == DayPhase.Night ? StaffOrderKind.Overtime : StaffOrderKind.Rest;
                var order = Session.Production.PreviewOrder(character.id, kind);
                string label = kind == StaffOrderKind.Rest ? LocalizationService.Format(T("workerRestButton"), order.allowed ? order.stressBefore - order.stressAfter : Session.Production.Config.restStressRecovery)
                    : LocalizationService.Format(T("overtimeButton"), Session.Production.Config.overtimeMoneyCost,
                        order.allowed ? order.stressAfter - order.stressBefore : Session.Production.Config.overtimeStress);
                if (Session.Production.HasOrder(character.id, kind))
                    label = T(kind == StaffOrderKind.Rest ? "restOrdered" : "overtimeOrdered");
                AddButton(choiceButtons, label, () => Session.Production.TryOrder(character.id, kind), order.allowed,
                    presentation.Icon(kind == StaffOrderKind.Rest ? "rest" : "overtime"));
            }
        }
        private void RenderPlanningCharacterMenu(CharacterDataSO character)
        {
            var interaction=Session.Interactions;var work=Session.Production;var person=Session.State.FindCharacter(character.id);
            int shared=Session.State.production.planning.shared.Any(s=>s.characterId==character.id&&s.cursor/4==Session.State.day-1)?0:Math.Min(100-person.affection,work.Config.planning.sharedAffection);
            var help=AddButton(choiceButtons,T("planningSharedAction"),()=>interaction.TryWork(),interaction.CanWork,presentation.Icon("work"));
            PlanningActionRow.Render(help,presentation,T("planningSharedAction"),new ActionMetric("stamina",-work.Config.helpCost),new ActionMetric("time",1),new ActionMetric("heart",shared));
            var talk=AddButton(choiceButtons,T("talk"),()=>interaction.TryTalk(),interaction.CanTalk,presentation.Icon("talk"));
            PlanningActionRow.Render(talk,presentation,T("talk"),new ActionMetric("stamina",-interaction.TalkCost),new ActionMetric("heart",interaction.RewardAvailable?Math.Min(100-person.affection,character.dailyTalkAffection):0));
            AddButton(choiceButtons,T("gift"),()=>{choosingGift=true;Refresh();},true,presentation.Icon("gift"));
            var kind=Session.State.phase==DayPhase.Night?StaffOrderKind.Overtime:StaffOrderKind.Rest;var f=work.PreviewOrder(character.id,kind);
            bool ordered=work.HasOrder(character.id,kind);
            string title=T(ordered?(kind==StaffOrderKind.Rest?"restOrdered":"overtimeOrdered"):(kind==StaffOrderKind.Rest?"planningRestAction":"planningOtAction"));
            var command=AddButton(choiceButtons,title,()=>work.TryOrder(character.id,kind),f.allowed,presentation.Icon(kind==StaffOrderKind.Rest?"rest":"overtime"));
            if(ordered)PlanningActionRow.Render(command,presentation,title);
            else if(kind==StaffOrderKind.Rest)PlanningActionRow.Render(command,presentation,title,new ActionMetric("stress",-Math.Min(person.stress,work.Config.restStressRecovery)),new ActionMetric("time",0));
            else PlanningActionRow.Render(command,presentation,title,new ActionMetric("money",-work.Config.overtimeMoneyCost),new ActionMetric("stress",Math.Min(100-person.stress,work.Config.overtimeStress)),new ActionMetric("time",0));
        }
        private void MakeScrollableChoices()
        {
            var content = (RectTransform)choiceButtons;
            var go = new GameObject("Choice Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect));
            var viewport = go.GetComponent<RectTransform>(); viewport.SetParent(content.parent, false);
            viewport.anchorMin = content.anchorMin; viewport.anchorMax = content.anchorMax;
            viewport.offsetMin = content.offsetMin; viewport.offsetMax = content.offsetMax;
            content.SetParent(viewport, false); content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.offsetMin = content.offsetMax = Vector2.zero;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = go.GetComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
        }
        private void BeginReveal()
        {
            if(IsEditorPreview||!Application.isPlaying){dialogueText.maxVisibleCharacters=int.MaxValue;return;}
            reveal=StartCoroutine(Reveal());
        }
        private IEnumerator Reveal()
        {
            IsRevealing = true;
            if (TextSpeed <= 0) { RevealAll(); yield break; }
            float visible = 0;
            while (dialogueText.maxVisibleCharacters < dialogueText.textInfo.characterCount)
            {
                visible += TextSpeed * Time.unscaledDeltaTime;
                dialogueText.maxVisibleCharacters = Mathf.FloorToInt(visible);
                yield return null;
            }
            IsRevealing = false; reveal = null;
        }
        internal Button AddButton(Transform parent, string title, Action callback, bool enabled, Sprite icon = null)
        {
            var list=parent.GetComponent<AuthoredButtonList>();
            var button = list!=null?list.Take(buttonTemplate):Instantiate(buttonTemplate, parent); button.gameObject.SetActive(true);
            button.interactable = enabled; Label(button, title);
            button.onClick.AddListener(() => { if (!Session.IsBusy || parent == choiceButtons) callback(); });
            if (presentation.useStudioSkin)
            {
                StudioUiWidgets.DecorateButton(button, presentation, icon, Session.Production.PlanningEnabled||Session.Romance.Enabled?null:title);
                return button;
            }
            if (icon != null)
            {
                var go = new GameObject("Item Icon", typeof(RectTransform), typeof(Image));
                var rect = go.GetComponent<RectTransform>(); rect.SetParent(button.transform, false);
                rect.anchorMin = new Vector2(.02f, .1f); rect.anchorMax = new Vector2(.12f, .9f); rect.offsetMin = rect.offsetMax = Vector2.zero;
                var image = go.GetComponent<Image>(); image.sprite = icon; image.preserveAspect = true; image.raycastTarget = false;
                button.GetComponentInChildren<TMP_Text>().rectTransform.anchorMin = new Vector2(.14f, .05f);
            }
            return button;
        }
        private static void Label(Button button, string value)
        {
            var text = button.GetComponentInChildren<TMP_Text>(true);
            text.text = value; if(button.GetComponent<AuthoredActionRow>()!=null)return;
            text.enableAutoSizing = true; text.fontSizeMin = 18; text.fontSizeMax = 24;
            text.textWrappingMode = TextWrappingModes.Normal;
        }
        private static void Clear(Transform parent)
        {
            var list=parent.GetComponent<AuthoredButtonList>();if(list!=null){list.Begin();return;}
            foreach (Transform child in parent) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        }
    }
}
