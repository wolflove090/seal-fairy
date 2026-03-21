using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FairyCatalogSource : MonoBehaviour
{
    [SerializeField] private List<FairyDefinition> fairies = new();

    public IReadOnlyList<FairyDefinition> GetFairies()
    {
        return fairies;
    }
}