using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardButton : MonoBehaviour
{
    [Min(0)] public int rewardAmount = 10; // pon aquí las monedas del nivel

    public void GiveReward()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoins(rewardAmount);
    }
}
