using UnityEngine;
using TMPro; // 👈 Importante para usar TextMeshProUGUI

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Puntaje actual (editable)")]
    public int score = 0;

    [Header("Referencia al texto del puntaje")]
    public TextMeshProUGUI scoreText; // 👈 Asigna el TMP de tu UI aquí

    const string SCORE_KEY = "score";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            int savedScore = PlayerPrefs.GetInt(SCORE_KEY, -1);
            if (savedScore >= 0) score = savedScore;

            Debug.Log("GameManager Awake. Score cargado = " + score);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddPoint()
    {
        score++;
        SaveScore();
        UpdateText(); // 👈 actualizar el texto cada vez que cambia el puntaje
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt(SCORE_KEY, score);
        PlayerPrefs.Save();
        Debug.Log("Score guardado: " + score);
    }

    public void UpdateText()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    [ContextMenu("Resetear puntaje")]
    public void ResetScore()
    {
        score = 0;
        SaveScore();

        if (scoreText != null)
            scoreText.text = "0"; // 👈 cambia el texto del TMP a "0"

        Debug.Log("Score reseteado y texto actualizado.");
    }
}
