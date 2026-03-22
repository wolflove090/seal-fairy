using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CurrencyBalanceSource : MonoBehaviour
{
    [SerializeField] private int startingBalance = 1000;

    public event Action<int> BalanceChanged;

    private int currentBalance;
    public int CurrentBalance
    {
        get => currentBalance;
        private set
        {
            currentBalance = value;
            BalanceChanged?.Invoke(currentBalance);
        }
    }

    private void Awake()
    {
        CurrentBalance = Mathf.Max(0, startingBalance);
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (CurrentBalance < amount)
        {
            return false;
        }

        CurrentBalance -= amount;
        BalanceChanged?.Invoke(CurrentBalance);
        return true;
    }

    public bool TryAdd(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        CurrentBalance += amount;
        BalanceChanged?.Invoke(CurrentBalance);
        return true;
    }
}
