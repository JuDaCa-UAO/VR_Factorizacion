using UnityEngine;
using System.Collections;
public class Fire : MonoBehaviour
{
    public string fireType;            // Tipo de fuego, define qu� extintor lo apaga
    public float extinguishPercentage;  // Valor porcentual de este fuego
    private bool isExtinguished = false; // Nueva bandera para evitar que se registre m�s de una vez

    [Header("Particle System")]
    public ParticleSystem fireParticles;  // Referencia al Particle System del fuego

    // M�todo para extinguir el fuego, que llamaremos desde el script Chorro
    public void Extinguish()
    {
        if (isExtinguished) return;  // Si el fuego ya ha sido extinguido, no hacer nada m�s

        // Informamos al GameManager cuando el fuego se extingue correctamente
        GameManager.Instance.RegisterFireExtinguished(this);

        // Marcar como extinguido
        isExtinguished = true;

        // Iniciar la animaci�n de reducci�n de tama�o de las part�culas
        StartCoroutine(ReducirParticulas());
    }

    private IEnumerator ReducirParticulas()
    {
        // Obtener el main module del ParticleSystem para controlar el tama�o
        var main = fireParticles.main;

        // Gradualmente reducir el tama�o de las part�culas
        float targetSize = 0.1f;  // Tama�o final de las part�culas (puedes ajustarlo)
        float currentSize = main.startSize.constant;
        float timeElapsed = 0f;
        float duration = 2f;  // Duraci�n de la animaci�n (puedes ajustarlo)

        while (timeElapsed < duration)
        {
            // Interpolaci�n entre el tama�o actual y el tama�o objetivo
            float newSize = Mathf.Lerp(currentSize, targetSize, timeElapsed / duration);
            main.startSize = newSize;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de que las part�culas han llegado al tama�o objetivo
        main.startSize = targetSize;

        // Despu�s de reducir el tama�o de las part�culas, desactivamos el fuego visualmente
        gameObject.SetActive(false);
    }
}