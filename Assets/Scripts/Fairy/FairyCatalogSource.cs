using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FairyCatalogSource : MonoBehaviour
{
    public IReadOnlyList<FairyDefinition> GetFairies()
    {
        return FairyCatalogRepository.GetFairies();
    }
}