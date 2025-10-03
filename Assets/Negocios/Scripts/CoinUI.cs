using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] string prefix = "Monedas: ";

    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            UpdateText(GameManager.Instance.Coins);
            GameManager.Instance.OnCoinsChanged += UpdateText;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnCoinsChanged -= UpdateText;
    }

    void UpdateText(int value)
    {
        if (label) label.text = prefix + value;
    }
}
