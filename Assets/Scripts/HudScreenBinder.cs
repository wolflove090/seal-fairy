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
    [SerializeField] private CurrencyBalanceSource currencyBalanceSource;
    [SerializeField] private TapStickerPlacer tapStickerPlacer;

    private readonly StickerSelectionState selectionState = new();
    private readonly List<(StickerDefinition sticker, VisualElement cell)> stickerCells = new();
    private readonly Vector2 previewLabelOffset = new(20f, -52f);

    private SealPhaseEventHub eventHub;
    private Button readyButton;
    private Button fairyButton;
    private Button shopButton;
    private VisualElement stickerPanel;
    private ScrollView stickerScrollView;
    private Label emptyStickerListLabel;
    private Label moneyLabel;
    private VisualElement root;
    private Label previewCountLabel;
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
    // private Label stickerShopMoneyLabel;
    private Button stickerShopCloseButton;
    private bool isSubscribed;
    private SealGamePhase currentPhase = SealGamePhase.StickerPlacement;

    public StickerSelectionState SelectionState => selectionState;
    public OwnedStickerInventorySource InventorySource => inventorySource;

    public void Initialize(SealPhaseEventHub eventHub)
    {
        UnsubscribeFromEventHub();
        this.eventHub = eventHub;
        SubscribeToEventHub();
        HandlePhaseChanged(SealGamePhase.StickerPlacement);
    }

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;
        moneyLabel = root.Q<Label>("money-label");
        readyButton = root.Q<Button>("ready-button");
        fairyButton = root.Q<Button>("fairy-button");
        shopButton = root.Q<Button>("shop-button");
        stickerPanel = root.Q<VisualElement>("bottom-left-sticker-panel");
        stickerScrollView = root.Q<ScrollView>("sticker-scroll-view");
        emptyStickerListLabel = root.Q<Label>("empty-sticker-list-label");
        previewCountLabel = root.Q<Label>("preview-count-label");

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

        SubscribeToInventorySource();
        SubscribeToCurrencySource();
        SubscribeToSelectionState();
        SubscribeToTapStickerPlacer();
        BuildStickerList();
        UpdateMoneyLabels();
        UpdateReadyButtonLabel();
        UpdateStickerPanelVisibility();
        UpdateAuxiliaryButtonVisibility();
        UpdatePreviewCountText();
        SetPreviewCountVisible(false);
        SubscribeToEventHub();
    }

    private void OnDisable()
    {
        UnsubscribeFromInventorySource();
        UnsubscribeFromCurrencySource();
        UnsubscribeFromSelectionState();
        UnsubscribeFromTapStickerPlacer();

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
        // stickerShopMoneyLabel = root.Q<Label>("sticker-shop-money-label");
        stickerShopCloseButton = root.Q<Button>("sticker-shop-close-button");

        if (stickerShopOverlay == null ||
            stickerShopBackdrop == null ||
            stickerShopPanel == null ||
            stickerShopScrollView == null ||
            stickerShopEmptyLabel == null ||
            // stickerShopMoneyLabel == null ||
            stickerShopCloseButton == null)
        {
            Debug.LogError("シールショップ UI の初期化に失敗しました");
            return;
        }

        stickerShopOverlay.style.display = DisplayStyle.None;
    }

    private void BuildStickerList()
    {
        stickerCells.Clear();
        stickerScrollView.Clear();

        IReadOnlyList<StickerDefinition> ownedStickers = inventorySource != null ? inventorySource.GetOwnedStickers() : null;
        StickerDefinition currentSelected = selectionState.SelectedSticker;
        selectionState.SetOwnedStickers(ownedStickers);

        if (ownedStickers == null || ownedStickers.Count == 0)
        {
            selectionState.ClearSelection();

            if (emptyStickerListLabel != null)
            {
                emptyStickerListLabel.style.display = DisplayStyle.Flex;
            }

            return;
        }

        if (emptyStickerListLabel != null)
        {
            emptyStickerListLabel.style.display = DisplayStyle.None;
        }

        foreach (StickerDefinition sticker in ownedStickers)
        {
            Button cell = CreateStickerCell(sticker);
            stickerCells.Add((sticker, cell));
            stickerScrollView.Add(cell);
        }

        bool stillOwned = false;
        foreach (StickerDefinition ownedSticker in ownedStickers)
        {
            if (ownedSticker == currentSelected)
            {
                stillOwned = true;
                break;
            }
        }

        if (currentSelected != null && stillOwned)
        {
            selectionState.Select(currentSelected);
        }
        else
        {
            selectionState.ClearSelection();
        }

        RefreshSelectionVisuals();
    }

    private void HandleOwnedStickersChanged()
    {
        if (!isActiveAndEnabled || stickerScrollView == null)
        {
            return;
        }

        BuildStickerList();
        UpdatePreviewCountText();
    }

    private void SubscribeToCurrencySource()
    {
        if (currencyBalanceSource == null)
        {
            Debug.LogError("CurrencyBalanceSource が設定されていません");
            return;
        }

        currencyBalanceSource.BalanceChanged -= HandleBalanceChanged;
        currencyBalanceSource.BalanceChanged += HandleBalanceChanged;
    }

    private void UnsubscribeFromCurrencySource()
    {
        if (currencyBalanceSource == null)
        {
            return;
        }

        currencyBalanceSource.BalanceChanged -= HandleBalanceChanged;
    }

    private void HandleBalanceChanged(int _)
    {
        UpdateMoneyLabels();

        if (stickerShopOverlay != null && stickerShopOverlay.style.display == DisplayStyle.Flex)
        {
            RefreshStickerShop();
        }
    }
    
    // 所持金表記の更新
    private void UpdateMoneyLabels()
    {
        RefreshMoneyLabelReferences();

        int balance = currencyBalanceSource != null ? currencyBalanceSource.CurrentBalance : 0;
        string text = balance.ToString("N0");

        if (moneyLabel != null)
        {
            moneyLabel.text = text;
        }
    }

    private void RefreshMoneyLabelReferences()
    {
        if (uiDocument == null)
        {
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        if (root == null)
        {
            return;
        }

        if (moneyLabel == null || moneyLabel.panel == null)
        {
            moneyLabel = root.Q<Label>("money-label");
        }
    }

    private Button CreateStickerCell(StickerDefinition sticker)
    {
        Button cell = new();
        cell.AddToClassList("sticker-cell");
        cell.tooltip = string.IsNullOrWhiteSpace(sticker?.DisplayName) ? "名称未設定" : sticker.DisplayName;

        VisualElement image = new();
        image.AddToClassList("sticker-cell__image");
        if (sticker != null && sticker.Icon != null)
        {
            image.style.backgroundImage = new StyleBackground(sticker.Icon.texture);
        }

        Label countLabel = new();
        countLabel.AddToClassList("sticker-cell__count");
        int count = inventorySource != null ? inventorySource.GetOwnedStickerCount(sticker) : 0;
        countLabel.text = $"x{count}";

        cell.Add(image);
        cell.Add(countLabel);
        cell.clicked += () => HandleStickerCellClicked(sticker);
        return cell;
    }

    private void HandleStickerCellClicked(StickerDefinition sticker)
    {
        selectionState.Select(sticker);
    }

    private void RefreshSelectionVisuals()
    {
        foreach ((StickerDefinition sticker, VisualElement cell) in stickerCells)
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
        UpdateAuxiliaryButtonVisibility();
        UpdatePreviewCountVisibility();

        if (currentPhase == SealGamePhase.StickerPeeling)
        {
            CloseFairyCollection();
            CloseStickerShop();
        }

        if(returnedToPlacement)
        {
            currencyBalanceSource?.TryAdd(500);
            selectionState.ClearSelection();
        }
    }

    private void UpdateStickerPanelVisibility()
    {
        if(stickerPanel == null)
            return;

        stickerPanel.style.display = currentPhase == SealGamePhase.StickerPlacement ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateAuxiliaryButtonVisibility()
    {
        DisplayStyle display = currentPhase == SealGamePhase.StickerPlacement ? DisplayStyle.Flex : DisplayStyle.None;

        if (fairyButton != null)
        {
            fairyButton.style.display = display;
        }

        if (shopButton != null)
        {
            shopButton.style.display = display;
        }
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
        UpdateMoneyLabels();
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
            UpdateMoneyLabels();
            return;
        }

        stickerShopEmptyLabel.style.display = DisplayStyle.None;
        UpdateMoneyLabels();

        foreach (StickerDefinition item in items)
        {
            stickerShopScrollView.Add(CreateStickerShopCard(item));
        }
    }

    // ショップカードの生成
    private VisualElement CreateStickerShopCard(StickerDefinition item)
    {
        VisualElement card = new();
        card.AddToClassList("sticker-shop-card");

        VisualElement imageFrame = new();
        imageFrame.AddToClassList("sticker-shop-card__image-frame");

        VisualElement image = new();
        image.AddToClassList("sticker-shop-card__image");
        if(item != null && item.Icon != null)
        {
            image.style.backgroundImage = new StyleBackground(item.Icon.texture);
        }

        Label name = new();
        name.AddToClassList("sticker-shop-card__name");
        name.text = string.IsNullOrWhiteSpace(item?.DisplayName) ? "*****" : item.DisplayName;

        Button pricePlate = new();
        pricePlate.AddToClassList("sticker-shop-card__price-plate");
        pricePlate.focusable = false;

        VisualElement coinIcon = new();
        coinIcon.AddToClassList("sticker-shop-card__coin-icon");

        Label price = new();
        price.AddToClassList("sticker-shop-card__price");
        price.text = item != null ? item.Price.ToString("N0") : "0";

        imageFrame.Add(image);
        imageFrame.Add(name);
        pricePlate.Add(coinIcon);
        pricePlate.Add(price);
        card.Add(imageFrame);
        card.Add(pricePlate);

        bool canPurchase = item != null &&
            currencyBalanceSource != null &&
            currencyBalanceSource.CurrentBalance >= item.Price;

        card.EnableInClassList("sticker-shop-card--disabled", !canPurchase);
        pricePlate.SetEnabled(canPurchase);

        if (canPurchase)
        {
            pricePlate.clicked += () => HandleStickerShopItemClicked(item);
        }

        return card;
    }

    private void HandleStickerShopItemClicked(StickerDefinition item)
    {
        if (item == null || inventorySource == null || currencyBalanceSource == null)
        {
            return;
        }

        if (!currencyBalanceSource.TrySpend(item.Price))
        {
            return;
        }

        inventorySource.AddOwnedStickerToFront(item);

        string displayName = string.IsNullOrWhiteSpace(item.DisplayName)
            ? "名称未設定"
            : item.DisplayName;

        Debug.Log($"ショップ購入: {displayName} / {item.Price}円 / 残高 {currencyBalanceSource.CurrentBalance}円");
        RefreshStickerShop();
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

    // 妖精コレクション画面の更新
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
            fairyCollectionCountLabel.text = "0/0";
            return;
        }

        fairyCollectionEmptyLabel.style.display = DisplayStyle.None;

        foreach(FairyDefinition fairy in fairies)
        {
            bool isDiscovered = fairy != null && FairyCollectionService.IsDiscovered(fairy.Id);
            fairyCollectionScrollView.Add(CreateFairyCard(fairy, isDiscovered));
        }

        fairyCollectionCountLabel.text = $"{FairyCollectionService.GetDiscoveredCount(fairies)}/{fairies.Count}";
    }

    // 妖精コレクションカードの作成
    private VisualElement CreateFairyCard(FairyDefinition fairy, bool isDiscovered)
    {
        VisualElement card = new();
        card.AddToClassList("fairy-card");

        Label nameLabel = new();
        nameLabel.AddToClassList("fairy-card__name");
        nameLabel.text = isDiscovered && fairy != null ? fairy.DisplayName : "？？？";

        VisualElement imageFrame = new();
        imageFrame.AddToClassList("fairy-card__image-frame");

        VisualElement image = new();
        image.AddToClassList("fairy-card__image");
        if(isDiscovered && fairy != null && fairy.Icon != null)
        {
            image.style.backgroundImage = new StyleBackground(fairy.Icon.texture);
        }
        else
        {
            image.AddToClassList("fairy-card__image--undiscovered");
        }

        Label detailLabel = new();
        detailLabel.AddToClassList("fairy-card__detail-label");
        detailLabel.text = "好きなシール：";

        Label detailValue = new();
        detailValue.AddToClassList("fairy-card__detail-value");
        detailValue.text = "クール/ワイルド";

        imageFrame.Add(image);
        card.Add(nameLabel);
        card.Add(imageFrame);
        card.Add(detailLabel);
        card.Add(detailValue);
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

    private void SubscribeToInventorySource()
    {
        if (inventorySource == null)
        {
            return;
        }

        inventorySource.OwnedStickersChanged -= HandleOwnedStickersChanged;
        inventorySource.OwnedStickersChanged += HandleOwnedStickersChanged;
    }

    private void UnsubscribeFromInventorySource()
    {
        if (inventorySource == null)
        {
            return;
        }

        inventorySource.OwnedStickersChanged -= HandleOwnedStickersChanged;
    }

    private void SubscribeToSelectionState()
    {
        selectionState.SelectedStickerChanged -= HandleSelectedStickerChanged;
        selectionState.SelectedStickerChanged += HandleSelectedStickerChanged;
    }

    private void UnsubscribeFromSelectionState()
    {
        selectionState.SelectedStickerChanged -= HandleSelectedStickerChanged;
    }

    private void HandleSelectedStickerChanged(StickerDefinition _)
    {
        RefreshSelectionVisuals();
        UpdatePreviewCountText();
        UpdatePreviewCountVisibility();
    }

    private void SubscribeToTapStickerPlacer()
    {
        if (tapStickerPlacer == null)
        {
            tapStickerPlacer = FindAnyObjectByType<TapStickerPlacer>();
        }

        if (tapStickerPlacer == null)
        {
            return;
        }

        tapStickerPlacer.PreviewScreenPointChanged -= HandlePreviewScreenPointChanged;
        tapStickerPlacer.PreviewScreenPointChanged += HandlePreviewScreenPointChanged;
    }

    private void UnsubscribeFromTapStickerPlacer()
    {
        if (tapStickerPlacer == null)
        {
            return;
        }

        tapStickerPlacer.PreviewScreenPointChanged -= HandlePreviewScreenPointChanged;
    }

    private void HandlePreviewScreenPointChanged(Vector2 screenPoint, bool visible)
    {
        if (previewCountLabel == null)
        {
            return;
        }

        if (!visible || currentPhase != SealGamePhase.StickerPlacement || selectionState.SelectedSticker == null)
        {
            SetPreviewCountVisible(false);
            return;
        }

        UpdatePreviewCountText();
        PositionPreviewCountLabel(screenPoint);
        SetPreviewCountVisible(true);
    }

    private void UpdatePreviewCountText()
    {
        if (previewCountLabel == null)
        {
            return;
        }

        StickerDefinition selectedSticker = selectionState.SelectedSticker;
        int count = inventorySource != null && selectedSticker != null
            ? inventorySource.GetOwnedStickerCount(selectedSticker)
            : 0;

        previewCountLabel.text = $"残り x{count}";
    }

    private void UpdatePreviewCountVisibility()
    {
        bool shouldShow =
            currentPhase == SealGamePhase.StickerPlacement &&
            selectionState.SelectedSticker != null &&
            inventorySource != null &&
            inventorySource.GetOwnedStickerCount(selectionState.SelectedSticker) > 0;

        if (!shouldShow)
        {
            SetPreviewCountVisible(false);
        }
    }

    private void PositionPreviewCountLabel(Vector2 screenPoint)
    {
        if (previewCountLabel == null || root == null)
        {
            return;
        }

        float x = screenPoint.x + previewLabelOffset.x;
        float y = Screen.height - screenPoint.y + previewLabelOffset.y;

        float rootWidth = root.resolvedStyle.width;
        float rootHeight = root.resolvedStyle.height;
        float labelWidth = previewCountLabel.resolvedStyle.width;
        float labelHeight = previewCountLabel.resolvedStyle.height;

        if (!float.IsNaN(rootWidth) && rootWidth > 0f && !float.IsNaN(labelWidth) && labelWidth > 0f)
        {
            x = Mathf.Clamp(x, 0f, Mathf.Max(0f, rootWidth - labelWidth));
        }

        if (!float.IsNaN(rootHeight) && rootHeight > 0f && !float.IsNaN(labelHeight) && labelHeight > 0f)
        {
            y = Mathf.Clamp(y, 0f, Mathf.Max(0f, rootHeight - labelHeight));
        }

        previewCountLabel.style.left = x;
        previewCountLabel.style.top = y;
    }

    private void SetPreviewCountVisible(bool visible)
    {
        if (previewCountLabel == null)
        {
            return;
        }

        previewCountLabel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
