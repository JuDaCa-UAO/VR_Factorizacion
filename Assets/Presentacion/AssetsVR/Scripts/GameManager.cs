using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalFiresExtinguished = 0;  // Contador total de fuegos apagados
    public List<float> extinguishedFirePercentages = new List<float>();  // Lista de porcentajes de fuegos apagados
    public float tiempoRestante;  // Variable para guardar el tiempo restante
    [SerializeField] public TextMeshProUGUI porcentajeFuegosText;  // Referencia al TextMeshProUGUI para mostrar el porcentaje

    // Variables estáticas para pasar datos entre escenas
    public static float tiempoFinal;
    public static float porcentajeFinal;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Persistir este objeto entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        ActualizarPorcentajeTexto();  // Actualizar el porcentaje en la interfaz
    }

    // Método para registrar cuando un fuego se apaga
    public void RegisterFireExtinguished(Fire fire)
    {
        totalFiresExtinguished++;
        extinguishedFirePercentages.Add(fire.extinguishPercentage);

        // Revisar el porcentaje total de fuegos apagados
        float totalPercentage = CalcularPorcentajeTotal();
        Debug.Log($"Porcentaje total de fuegos apagados: {totalPercentage}%");

        // Si el porcentaje de incendios apagados llega al 100%, detener el timer y guardar el tiempo
        if (totalPercentage >= 100f)
        {
            if (Timer.Instance != null)
            {
                Timer.Instance.StopTimer();  // Detener el temporizador
            }

            GuardarTiempo(Timer.Instance.GetTiempoRestante());  // Guardar el tiempo restante
            CambiarEscenaWin();  // Cambiar a la escena Win
        }
    }

    // Método para calcular el porcentaje total de fuegos apagados
    public float CalcularPorcentajeTotal()
    {
        float total = 0f;
        foreach (float percentage in extinguishedFirePercentages)
        {
            total += percentage;
        }
        return total;
    }

    private void ActualizarPorcentajeTexto()
    {
        float porcentajeActual = CalcularPorcentajeTotal();
        if (porcentajeFuegosText != null)
        {
            porcentajeFuegosText.text = $"Porcentaje de Fuegos Apagados: {porcentajeActual}%";
        }
    }

    // Método para guardar el tiempo restante desde el Timer
    public void GuardarTiempo(float tiempo)
    {
        tiempoRestante = tiempo;
        tiempoFinal = tiempoRestante;  // Guardar el tiempo final
        Debug.Log($"Tiempo guardado: {tiempoRestante}");
    }

    // Método para validar la victoria: si el porcentaje es >= 70 y el tiempo es 0, cambiar de escena
    public void ValidarVictoria()
    {
        float totalPercentage = CalcularPorcentajeTotal();
        if (totalPercentage >= 70f)  // Si el porcentaje es 70% o más
        {
            CambiarEscenaWin();  // Cambiar a la escena Win
        }
    }

    // Método para cambiar de escena a Win
    public void CambiarEscenaWin()
    {
        porcentajeFinal = CalcularPorcentajeTotal();  // Guardar el porcentaje final
        SceneManager.LoadScene("Win");  // Cargar la escena Win
    }
}