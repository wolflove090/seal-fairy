using UnityEngine;

public sealed class SealPhaseController : MonoBehaviour
{
    [SerializeField] private TapStickerPlacer tapStickerPlacer;

    public SealGamePhase CurrentPhase { get; private set; } = SealGamePhase.StickerPlacement;

    private SealPhaseEventHub eventHub;
    private bool isPeelingLocked;
    private bool isSubscribed;

    public void Initialize(SealPhaseEventHub eventHub)
    {
        UnsubscribeFromEventHub();
        this.eventHub = eventHub;
        SubscribeToEventHub();
    }

    public void SetSelectionState(StickerSelectionState selectionState)
    {
        tapStickerPlacer.SetSelectionState(selectionState);
    }

    private void OnEnable()
    {
        SubscribeToEventHub();
    }

    private void Start()
    {
        ApplyPhase(SealGamePhase.StickerPlacement);
    }

    private void OnDisable()
    {
        UnsubscribeFromEventHub();
    }

    private void HandlePhaseToggleRequested()
    {
        if (CurrentPhase == SealGamePhase.StickerPlacement)
        {
            ApplyPhase(SealGamePhase.StickerPeeling);
            return;
        }

        ClearRemainingStickers();
        ApplyPhase(SealGamePhase.StickerPlacement);
    }

    public void SetPeelingLocked(bool locked)
    {
        isPeelingLocked = locked;
        ApplyTapPeelState();
    }

    private void ApplyPhase(SealGamePhase phase)
    {
        CurrentPhase = phase;
        tapStickerPlacer.SetPlacementEnabled(phase == SealGamePhase.StickerPlacement);
        ApplyTapPeelState();

        eventHub?.NotifyPhaseChanged(CurrentPhase);
    }

    private void ClearRemainingStickers()
    {
        isPeelingLocked = false;

        foreach (PeelSticker3D sticker in StickerRuntimeRegistry.GetActiveStickers())
        {
            if (sticker != null)
            {
                Destroy(sticker.gameObject);
            }
        }

        StickerRuntimeRegistry.ClearAll();
    }

    private void ApplyTapPeelState()
    {
        bool canPeel = CurrentPhase == SealGamePhase.StickerPeeling && !isPeelingLocked;

        foreach (PeelSticker3D sticker in StickerRuntimeRegistry.GetActiveStickers())
        {
            sticker.SetTapPeelEnabled(canPeel);
        }
    }

    private void SubscribeToEventHub()
    {
        if (isSubscribed || eventHub == null || !isActiveAndEnabled)
        {
            return;
        }

        eventHub.PhaseToggleRequested += HandlePhaseToggleRequested;
        isSubscribed = true;
    }

    private void UnsubscribeFromEventHub()
    {
        if (!isSubscribed || eventHub == null)
        {
            return;
        }

        eventHub.PhaseToggleRequested -= HandlePhaseToggleRequested;
        isSubscribed = false;
    }
}
