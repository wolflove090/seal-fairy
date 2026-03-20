using UnityEngine;
using UnityEngine.UIElements;

public sealed class HubScreenBinder : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private SealPhaseEventHub eventHub;
    private Button readyButton;
    private bool isSubscribed;
    private SealGamePhase currentPhase = SealGamePhase.StickerPlacement;

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

        if (readyButton == null)
        {
            UnityEngine.Debug.LogError("ready-button が見つかりません");
            return;
        }

        readyButton.clicked += HandleReadyButtonClicked;
        UpdateReadyButtonLabel();
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

    private void HandleReadyButtonClicked()
    {
        eventHub?.RequestPhaseToggle();
    }

    private void HandlePhaseChanged(SealGamePhase phase)
    {
        currentPhase = phase;
        UpdateReadyButtonLabel();
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

    private void UpdateReadyButtonLabel()
    {
        if (readyButton == null)
        {
            return;
        }

        readyButton.text = currentPhase == SealGamePhase.StickerPlacement ? "シールめくりへ" : "シール配置へ";
    }
}
