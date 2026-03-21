using UnityEngine;

public sealed class SealPhaseBoostrap : MonoBehaviour
{
    [SerializeField] private HubScreenBinder hubScreenBinder;
    [SerializeField] private SealPhaseController sealPhaseController;

    private void Awake()
    {
        SealPhaseEventHub eventHub = new();
        hubScreenBinder.Initialize(eventHub);
        sealPhaseController.SetSelectionState(hubScreenBinder.SelectionState);
        sealPhaseController.Initialize(eventHub);
    }
}
