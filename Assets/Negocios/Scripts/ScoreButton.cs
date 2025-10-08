using UnityEngine;
using TMPro;

public class ScoreButton : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // si por alguna razón no hay GameManager (ej. arrancaste Play en esta escena),
        // usamos PlayerPrefs como respaldo.
        if (GameManager.Instance == null)
        {
            int s = PlayerPrefs.GetInt("score", 0);
            scoreText.text = s.ToString();
            Debug.LogWarning("No GameManager instancia. Mostrar score desde PlayerPrefs: " + s);
            return;
        }
        Refresh();
    }

    public void AddPoint()
    {
        if (GameManager.Instance == null)
        {
            // respaldo: sumar directamente en PlayerPrefs si no hay GameManager
            int s = PlayerPrefs.GetInt("score", 0) + 1;
            PlayerPrefs.SetInt("score", s);
            PlayerPrefs.Save();
            scoreText.text = s.ToString();
            return;
        }

        GameManager.Instance.AddPoint();
        Refresh();
    }

    void Refresh()
    {
        scoreText.text = GameManager.Instance.score.ToString();
    }
}
