using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class HubScreenBinder : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private OwnedStickerInventorySource inventorySource;
    [SerializeField] private FairyCatalogSource fairyCatalogSource;
    [SerializeField] private VisualTreeAsset fairyCollectionScreenAsset;
    [SerializeField] private StickerShopCatalogSource stickerShopCatalogSource;
    [SerializeField] private VisualTreeAsset stickerShopScreenAsset;

    private readonly StickerSelectionState selectionState = new();
    private readonly Dictionary<StickerDefinition, VisualElement> stickerCellByDefinition = new();

    private SealPhaseEventHub eventHub;
    private Button readyButton;
    private Button fairyButton;
    private Button shopButton;
    private VisualElement stickerPanel;
    private ScrollView stickerScrollView;
    private Label emptyStickerListLabel;
    private VisualElement fairyCollectionOverlay;
    private Button fairyCollectionBackdrop;
    private VisualElement fairyCollectionPanel;
    private ScrollView fairyCollectionScrollView;
    private Label fairyCollectionEmptyLabel;
    private Label fairyCollectionCountLabel;
    private Button fairyCollectionCloseButton;
    private VisualElement stickerShopOverlay;
    private Button stickerShopBackdrop;
    private VisualElement stickerShopPanel;
    private ScrollView stickerShopScrollView;
    private Label stickerShopEmptyLabel;
    private Label stickerShopMoneyLabel;
    private Button stickerShopCloseButton;
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
        fairyButton = root.Q<Button>("fairy-button");
        shopButton = root.Q<Button>("shop-button");
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

        InitializeFairyCollectionUi(root);
        InitializeStickerShopUi(root);

        readyButton.clicked += HandleReadyButtonClicked;
        if (fairyButton != null)
        {
            fairyButton.clicked += OpenFairyCollection;
        }

        if (shopButton != null)
        {
            shopButton.clicked += OpenStickerShop;
        }

        if (fairyCollectionCloseButton != null)
        {
            fairyCollectionCloseButton.clicked += CloseFairyCollection;
        }

        if (fairyCollectionBackdrop != null)
        {
            fairyCollectionBackdrop.clicked += CloseFairyCollection;
        }

        if (stickerShopCloseButton != null)
        {
            stickerShopCloseButton.clicked += CloseStickerShop;
        }

        if (stickerShopBackdrop != null)
        {
            stickerShopBackdrop.clicked += CloseStickerShop;
        }

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

        if (fairyButton != null)
        {
            fairyButton.clicked -= OpenFairyCollection;
        }

        if (shopButton != null)
        {
            shopButton.clicked -= OpenStickerShop;
        }

        if (fairyCollectionCloseButton != null)
        {
            fairyCollectionCloseButton.clicked -= CloseFairyCollection;
        }

        if (fairyCollectionBackdrop != null)
        {
            fairyCollectionBackdrop.clicked -= CloseFairyCollection;
        }

        if (stickerShopCloseButton != null)
        {
            stickerShopCloseButton.clicked -= CloseStickerShop;
        }

        if (stickerShopBackdrop != null)
        {
            stickerShopBackdrop.clicked -= CloseStickerShop;
        }

        UnsubscribeFromEventHub();
    }

    private void InitializeFairyCollectionUi(VisualElement root)
    {
        if (root == null || fairyCollectionScreenAsset == null)
        {
            return;
        }

        fairyCollectionOverlay = root.Q<VisualElement>("fairy-collection-overlay");
        if (fairyCollectionOverlay == null)
        {
            fairyCollectionScreenAsset.CloneTree(root);
            fairyCollectionOverlay = root.Q<VisualElement>("fairy-collection-overlay");
        }

        fairyCollectionBackdrop = root.Q<Button>("fairy-collection-backdrop");
        fairyCollectionPanel = root.Q<VisualElement>("fairy-collection-panel");
        fairyCollectionScrollView = root.Q<ScrollView>("fairy-collection-scroll-view");
        fairyCollectionEmptyLabel = root.Q<Label>("fairy-collection-empty-label");
        fairyCollectionCountLabel = root.Q<Label>("fairy-collection-count-label");
        fairyCollectionCloseButton = root.Q<Button>("fairy-collection-close-button");

        if (fairyCollectionOverlay == null ||
            fairyCollectionBackdrop == null ||
            fairyCollectionPanel == null ||
            fairyCollectionScrollView == null ||
            fairyCollectionEmptyLabel == null ||
            fairyCollectionCountLabel == null ||
            fairyCollectionCloseButton == null)
        {
            Debug.LogError("妖精コレクション UI の初期化に失敗しました");
            return;
        }

        fairyCollectionOverlay.style.display = DisplayStyle.None;
    }

    private void InitializeStickerShopUi(VisualElement root)
    {
        if (root == null || stickerShopScreenAsset == null)
        {
            return;
        }

        stickerShopOverlay = root.Q<VisualElement>("sticker-shop-overlay");
        if (stickerShopOverlay == null)
        {
            stickerShopScreenAsset.CloneTree(root);
            stickerShopOverlay = root.Q<VisualElement>("sticker-shop-overlay");
        }

        stickerShopBackdrop = root.Q<Button>("sticker-shop-backdrop");
        stickerShopPanel = root.Q<VisualElement>("sticker-shop-panel");
        stickerShopScrollView = root.Q<ScrollView>("sticker-shop-scroll-view");
        stickerShopEmptyLabel = root.Q<Label>("sticker-shop-empty-label");
        stickerShopMoneyLabel = root.Q<Label>("sticker-shop-money-label");
        stickerShopCloseButton = root.Q<Button>("sticker-shop-close-button");

        if (stickerShopOverlay == null ||
            stickerShopBackdrop == null ||
            stickerShopPanel == null ||
            stickerShopScrollView == null ||
            stickerShopEmptyLabel == null ||
            stickerShopMoneyLabel == null ||
            stickerShopCloseButton == null)
        {
            Debug.LogError("シールショップ UI の初期化に失敗しました");
            return;
        }

        stickerShopOverlay.style.display = DisplayStyle.None;
    }

    private void BuildStickerList()
    {
        stickerCellByDefinition.Clear();
        stickerScrollView.Clear();

        IReadOnlyList<StickerDefinition> ownedStickers = inventorySource != null ? inventorySource.GetOwnedStickers() : null;

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

        foreach(StickerDefinition sticker in ownedStickers)
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

    private Button CreateStickerCell(StickerDefinition sticker)
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

    private void HandleStickerCellClicked(StickerDefinition sticker)
    {
        selectionState.Select(sticker);
        RefreshSelectionVisuals();
    }

    private void RefreshSelectionVisuals()
    {
        foreach((StickerDefinition sticker, VisualElement cell) in stickerCellByDefinition)
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

    // ========== ステッカーショップ ========== //

    private void OpenStickerShop()
    {
        if (stickerShopOverlay == null || stickerShopScrollView == null)
        {
            return;
        }

        CloseFairyCollection();
        RefreshStickerShop();
        stickerShopOverlay.style.display = DisplayStyle.Flex;
    }

    private void CloseStickerShop()
    {
        if (stickerShopOverlay == null)
        {
            return;
        }

        stickerShopOverlay.style.display = DisplayStyle.None;
    }

    private void RefreshStickerShop()
    {
        if (stickerShopScrollView == null || stickerShopEmptyLabel == null)
        {
            return;
        }

        IReadOnlyList<StickerDefinition> items = stickerShopCatalogSource != null ? stickerShopCatalogSource.GetItems() : null;

        stickerShopScrollView.Clear();

        if (items == null || items.Count == 0)
        {
            stickerShopEmptyLabel.style.display = DisplayStyle.Flex;
            return;
        }

        stickerShopEmptyLabel.style.display = DisplayStyle.None;

        foreach (StickerDefinition item in items)
        {
            stickerShopScrollView.Add(CreateStickerShopCard(item));
        }
    }

    private Button CreateStickerShopCard(StickerDefinition item)
    {
        Button card = new();
        card.AddToClassList("sticker-shop-card");

        VisualElement image = new();
        image.AddToClassList("sticker-shop-card__image");
        if(item != null && item.Icon != null)
        {
            image.style.backgroundImage = new StyleBackground(item.Icon.texture);
        }

        Label name = new();
        name.AddToClassList("sticker-shop-card__name");
        name.text = string.IsNullOrWhiteSpace(item?.DisplayName) ? "名称未設定" : item.DisplayName;

        card.Add(image);
        card.Add(name);
        card.clicked += () => Debug.Log($"ショップシール選択: {name.text}");
        return card;
    }

    // ========== 妖精一覧 ========== //

    private void OpenFairyCollection()
    {
        if (fairyCollectionOverlay == null || fairyCollectionScrollView == null)
        {
            return;
        }

        CloseStickerShop();
        RefreshFairyCollection();
        fairyCollectionOverlay.style.display = DisplayStyle.Flex;
    }

    private void CloseFairyCollection()
    {
        if (fairyCollectionOverlay == null)
        {
            return;
        }

        fairyCollectionOverlay.style.display = DisplayStyle.None;
    }

    private void RefreshFairyCollection()
    {
        if (fairyCollectionScrollView == null || fairyCollectionEmptyLabel == null || fairyCollectionCountLabel == null)
        {
            return;
        }

        IReadOnlyList<FairyDefinition> fairies = fairyCatalogSource != null ? fairyCatalogSource.GetFairies() : null;

        fairyCollectionScrollView.Clear();

        if(fairies == null || fairies.Count == 0)
        {
            fairyCollectionEmptyLabel.style.display = DisplayStyle.Flex;
            fairyCollectionCountLabel.text = "発見した数: 0/0";
            return;
        }

        fairyCollectionEmptyLabel.style.display = DisplayStyle.None;

        foreach(FairyDefinition fairy in fairies)
        {
            bool isDiscovered = fairy != null && FairyCollectionService.IsDiscovered(fairy.Id);
            fairyCollectionScrollView.Add(CreateFairyCard(fairy, isDiscovered));
        }

        fairyCollectionCountLabel.text = $"発見した数: {FairyCollectionService.GetDiscoveredCount(fairies)}/{fairies.Count}";
    }

    private VisualElement CreateFairyCard(FairyDefinition fairy, bool isDiscovered)
    {
        VisualElement card = new();
        card.AddToClassList("fairy-card");

        Label nameLabel = new();
        nameLabel.AddToClassList("fairy-card__name");
        nameLabel.text = isDiscovered && fairy != null ? fairy.DisplayName : "？？？";

        VisualElement image = new();
        image.AddToClassList("fairy-card__image");
        if(!isDiscovered)
        {
            image.AddToClassList("fairy-card__image--undiscovered");
        }
        else if(fairy != null && fairy.Icon != null)
        {
            image.style.backgroundImage = new StyleBackground(fairy.Icon.texture);
        }

        Label detailLabel = new();
        detailLabel.AddToClassList("fairy-card__detail");
        detailLabel.text = isDiscovered ? "好きなシール: ポップ・小さい\n妖精の説明" : "好きなシール: ポップ・小さい\n未発見";

        card.Add(nameLabel);
        card.Add(image);
        card.Add(detailLabel);
        return card;
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
