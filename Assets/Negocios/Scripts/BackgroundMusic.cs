using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [Header("Música de fondo")]
    public AudioClip backgroundMusic; // Asigna tu música de fondo en el Inspector

    private AudioSource audioSource;

    void Awake()
    {
        // Obtener el componente AudioSource
        audioSource = GetComponent<AudioSource>();

        // Verifica que la música de fondo esté asignada
        if (backgroundMusic == null)
        {
            Debug.LogError("[BackgroundMusic] No se ha asignado una música de fondo.");
            return;
        }

        // Configurar el AudioSource
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;  // Activar el loop
        audioSource.playOnAwake = true;  // Reproducir la música al iniciar

        // Reproducir la música de fondo
        audioSource.Play();
    }
}
