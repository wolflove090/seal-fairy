using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class HubScreenBinder : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private OwnedStickerInventorySource inventorySource;
    
    private readonly StickerSelectionState selectionState = new();
    private readonly Dictionary<OwnedStickerDefinition, VisualElement> stickerCellByDefinition = new();

    private SealPhaseEventHub eventHub;
    private Button readyButton;
    private VisualElement stickerPanel;
    private ScrollView stickerScrollView;
    private Label emptyStickerListLabel;
    private bool isSubscribed;
    private bool hasAppliedInitialSelection;
    private SealGamePhase currentPhase = SealGamePhase.StickerPlacement;

    public StickerSelectionState SelectionState => selectionState;

    public void Initialize(SealPhaseEventHub eventHub)
    {
        UnsubscribeFromEventHub();
        this.eventHub = eventHub;
        SubscribeToEventHub();
        HandlePhaseChanged(SealGamePhase.StickerPlacement);
    }

    private void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;
        readyButton = root.Q<Button>("ready-button");
        stickerPanel = root.Q<VisualElement>("bottom-left-sticker-panel");
        stickerScrollView = root.Q<ScrollView>("sticker-scroll-view");
        emptyStickerListLabel = root.Q<Label>("empty-sticker-list-label");

        if (readyButton == null)
        {
            UnityEngine.Debug.LogError("ready-button が見つかりません");
            return;
        }

        if (stickerPanel == null || stickerScrollView == null)
        {
            Debug.LogError("シール一覧 UI が見つかりません");
            return;
        }

        readyButton.clicked += HandleReadyButtonClicked;
        BuildStickerList();
        UpdateReadyButtonLabel();
        UpdateStickerPanelVisibility();
        SubscribeToEventHub();
    }

    private void OnDisable()
    {
        if (readyButton != null)
        {
            readyButton.clicked -= HandleReadyButtonClicked;
        }

        UnsubscribeFromEventHub();
    }

    private void BuildStickerList()
    {
        stickerCellByDefinition.Clear();
        stickerScrollView.Clear();

        IReadOnlyList<OwnedStickerDefinition> ownedStickers = inventorySource != null ? inventorySource.GetOwnedStickers() : null;

        selectionState.SetOwnedStickers(ownedStickers);

        if(ownedStickers == null || ownedStickers.Count == 0)
        {
            if(emptyStickerListLabel != null)
            {
                emptyStickerListLabel.style.display = DisplayStyle.Flex;
            }
            return;
        }

        if(emptyStickerListLabel != null)
        {
            emptyStickerListLabel.style.display = DisplayStyle.None;
        }

        foreach(OwnedStickerDefinition sticker in ownedStickers)
        {
            Button cell = CreateStickerCell(sticker);
            stickerCellByDefinition.Add(sticker, cell);
            stickerScrollView.Add(cell);
        }

        if(!hasAppliedInitialSelection)
        {
            selectionState.SelectInitialSticker();
            hasAppliedInitialSelection = true;
        }

        RefreshSelectionVisuals();
    }

    private Button CreateStickerCell(OwnedStickerDefinition sticker)
    {
        Button cell = new();
        cell.AddToClassList("sticker-cell");

        VisualElement image = new();
        image.AddToClassList("sticker-cell__image");
        if(sticker.Icon != null)
        {
            image.style.backgroundImage = new StyleBackground(sticker.Icon.texture);
        }

        cell.Add(image);
        cell.clicked += () => HandleStickerCellClicked(sticker);
        return cell;
    }

    private void HandleStickerCellClicked(OwnedStickerDefinition sticker)
    {
        selectionState.Select(sticker);
        RefreshSelectionVisuals();
    }

    private void RefreshSelectionVisuals()
    {
        foreach((OwnedStickerDefinition sticker, VisualElement cell) in stickerCellByDefinition)
        {
            cell.EnableInClassList("sticker-cell--selected", selectionState.SelectedSticker == sticker);
        }
    }

    private void HandleReadyButtonClicked()
    {
        eventHub?.RequestPhaseToggle();
    }

    private void HandlePhaseChanged(SealGamePhase phase)
    {
        bool returnedToPlacement = currentPhase == SealGamePhase.StickerPeeling && phase == SealGamePhase.StickerPlacement;
        
        currentPhase = phase;
        UpdateReadyButtonLabel();
        UpdateStickerPanelVisibility();

        if(returnedToPlacement)
        {
            selectionState.ClearSelection();
            RefreshSelectionVisuals();
        }
    }

    private void UpdateStickerPanelVisibility()
    {
        if(stickerPanel == null)
            return;

        stickerPanel.style.display = currentPhase == SealGamePhase.StickerPlacement ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateReadyButtonLabel()
    {
        if (readyButton == null)
        {
            return;
        }

        readyButton.text = currentPhase == SealGamePhase.StickerPlacement ? "シールめくりへ" : "シール配置へ";
    }

    private void SubscribeToEventHub()
    {
        if (isSubscribed || eventHub == null || !isActiveAndEnabled)
        {
            return;
        }

        eventHub.PhaseChanged += HandlePhaseChanged;
        isSubscribed = true;
    }

    private void UnsubscribeFromEventHub()
    {
        if (!isSubscribed || eventHub == null)
        {
            return;
        }

        eventHub.PhaseChanged -= HandlePhaseChanged;
        isSubscribed = false;
    }
}
