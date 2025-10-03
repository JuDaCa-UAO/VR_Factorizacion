using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public static Timer Instance;  // Referencia est�tica al Timer
    public float tiempoTotal = 10f;  // Tiempo total en segundos
    private float tiempoRestante;
    public TextMeshProUGUI textoTiempo;  // Objeto de texto UI para mostrar el tiempo

    private bool timerActivo = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        tiempoRestante = tiempoTotal;
        IniciarTimer();
    }

    void Update()
    {
        if (timerActivo && tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            textoTiempo.text = "Tiempo: " + Mathf.Ceil(tiempoRestante).ToString();

            if (tiempoRestante <= 0)
            {
                timerActivo = false;
                tiempoRestante = 0;
                ComprobarVictoria();  // Llamar a la validaci�n de victoria cuando el tiempo llegue a 0
            }
        }
    }

    // M�todo para iniciar el temporizador
    public void IniciarTimer()
    {
        timerActivo = true;
    }

    // M�todo para detener el temporizador (cuando se alcanza el 100% de fuegos apagados)
    public void StopTimer()
    {
        timerActivo = false;
        Debug.Log("Temporizador detenido.");
    }

    // M�todo para obtener el tiempo restante
    public float GetTiempoRestante()
    {
        return tiempoRestante;
    }

    // M�todo para comprobar si se cumple la victoria (porcentaje >= 70%)
    private void ComprobarVictoria()
    {
        if (GameManager.Instance != null)
        {
            float porcentajeTotal = GameManager.Instance.CalcularPorcentajeTotal();
            Debug.Log($"Porcentaje de incendios apagados: {porcentajeTotal}%");

            // Si el porcentaje es mayor o igual al 70%, cambiamos a la escena de victoria
            if (porcentajeTotal >= 70f)
            {
                GameManager.Instance.GuardarTiempo(tiempoRestante);  // Guardamos el tiempo restante
                GameManager.Instance.CambiarEscenaWin();  // Cambiar a la escena Win
            }
            else
            {
                Debug.Log("No se alcanz� el porcentaje m�nimo para ganar.");
            }
        }
    }
}