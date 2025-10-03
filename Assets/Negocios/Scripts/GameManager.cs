using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Coins { get; private set; }
    public event Action<int> OnCoinsChanged;

    [SerializeField] bool loadAndSavePlayerPrefs = true;
    const string CoinsKey = "GM_COINS";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadAndSavePlayerPrefs)
            Coins = PlayerPrefs.GetInt(CoinsKey, 0);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
        if (loadAndSavePlayerPrefs) PlayerPrefs.SetInt(CoinsKey, Coins);
    }

    public void ResetCoins()
    {
        Coins = 0;
        OnCoinsChanged?.Invoke(Coins);
        if (loadAndSavePlayerPrefs) PlayerPrefs.SetInt(CoinsKey, Coins);
    }
}