using UnityEngine;

public class FireExtinguish : MonoBehaviour
{
    private ParticleSystem fuego;

    void Start()
    {
        fuego = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Water"))
        {
            fuego.Stop(); // Detiene el sistema de part�culas del fuego
            Destroy(gameObject, 2f); // Opcional: destruye el objeto despu�s de 2 segundos
        }
    }
}
