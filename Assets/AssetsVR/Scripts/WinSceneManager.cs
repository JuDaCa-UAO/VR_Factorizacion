using UnityEngine;
using TMPro;

public class WinSceneManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tiempoText;  // Referencia al TextMeshPro para el tiempo
    [SerializeField] private TextMeshProUGUI porcentajeText;  // Referencia al TextMeshPro para el porcentaje

    void Start()
    {
        // Redondear el tiempo a dos decimales
        float tiempoRedondeado = Mathf.Round(GameManager.tiempoFinal);

        // Mostrar los datos guardados de la escena anterior
        tiempoText.text = $"Tiempo restante: {tiempoRedondeado} segundos";
        porcentajeText.text = $"Porcentaje de Fuegos Apagados: {GameManager.porcentajeFinal}%";
    }
}