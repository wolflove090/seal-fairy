using System;
using UnityEngine;

public sealed class SealPhaseEventHub
{
    public event Action PhaseToggleRequested;
    public event Action<SealGamePhase> PhaseChanged;

    public void RequestPhaseToggle()
    {
        PhaseToggleRequested?.Invoke();
    }

    public void NotifyPhaseChanged(SealGamePhase phase)
    {
        PhaseChanged?.Invoke(phase);
    }
}