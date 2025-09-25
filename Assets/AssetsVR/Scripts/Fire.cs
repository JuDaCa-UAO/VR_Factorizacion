using UnityEngine;
using System.Collections;
public class Fire : MonoBehaviour
{
    public string fireType;            // Tipo de fuego, define qué extintor lo apaga
    public float extinguishPercentage;  // Valor porcentual de este fuego
    private bool isExtinguished = false; // Nueva bandera para evitar que se registre más de una vez

    [Header("Particle System")]
    public ParticleSystem fireParticles;  // Referencia al Particle System del fuego

    // Método para extinguir el fuego, que llamaremos desde el script Chorro
    public void Extinguish()
    {
        if (isExtinguished) return;  // Si el fuego ya ha sido extinguido, no hacer nada más

        // Informamos al GameManager cuando el fuego se extingue correctamente
        GameManager.Instance.RegisterFireExtinguished(this);

        // Marcar como extinguido
        isExtinguished = true;

        // Iniciar la animación de reducción de tamaño de las partículas
        StartCoroutine(ReducirParticulas());
    }

    private IEnumerator ReducirParticulas()
    {
        // Obtener el main module del ParticleSystem para controlar el tamaño
        var main = fireParticles.main;

        // Gradualmente reducir el tamaño de las partículas
        float targetSize = 0.1f;  // Tamaño final de las partículas (puedes ajustarlo)
        float currentSize = main.startSize.constant;
        float timeElapsed = 0f;
        float duration = 2f;  // Duración de la animación (puedes ajustarlo)

        while (timeElapsed < duration)
        {
            // Interpolación entre el tamaño actual y el tamaño objetivo
            float newSize = Mathf.Lerp(currentSize, targetSize, timeElapsed / duration);
            main.startSize = newSize;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de que las partículas han llegado al tamaño objetivo
        main.startSize = targetSize;

        // Después de reducir el tamaño de las partículas, desactivamos el fuego visualmente
        gameObject.SetActive(false);
    }
}