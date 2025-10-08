using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Puntaje actual (editable)")]
    public int score = 0;

    const string SCORE_KEY = "score";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cargar el puntaje guardado solo si aún no lo has modificado en el Inspector
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
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt(SCORE_KEY, score);
        PlayerPrefs.Save();
        Debug.Log("Score guardado: " + score);
    }

    // 🔹 Puedes llamar esto desde el Inspector para reiniciar o cambiar manualmente
    [ContextMenu("Resetear puntaje")]
    public void ResetScore()
    {
        score = 0;
        SaveScore();
        Debug.Log("Score reseteado.");
    }
}
