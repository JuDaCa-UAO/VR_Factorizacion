using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class PlayVideoAndChangeScene : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;  // VideoPlayer que reproduce el video

    [Header("Escena siguiente")]
    [SerializeField] private string nextSceneName;     // Nombre de la escena a cargar al terminar el video

    [Header("Opcional")]
    [SerializeField] private bool playOnStart = true;  // ¿Reproducir apenas se cargue la escena?

    private bool alreadyTriggered = false;

    private void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[PlayVideoAndChangeScene] No se asignó el VideoPlayer.");
            return;
        }

        // Nos suscribimos al evento cuando el video termina
        videoPlayer.loopPointReached += OnVideoFinished;

        if (playOnStart)
        {
            // Por seguridad, nos aseguramos de que esté activo y reproduciendo
            if (!videoPlayer.gameObject.activeSelf)
                videoPlayer.gameObject.SetActive(true);

            videoPlayer.Play();
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (alreadyTriggered) return; // evitar llamarlo dos veces
        alreadyTriggered = true;

        Debug.Log("[PlayVideoAndChangeScene] Video finalizado, cambiando de escena...");

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[PlayVideoAndChangeScene] nextSceneName está vacío, asigna el nombre de la escena en el inspector.");
        }
    }

    // Si NO quieres playOnStart, puedes llamar a este método desde otro script o botón
    public void PlayVideoManually()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[PlayVideoAndChangeScene] No se asignó el VideoPlayer.");
            return;
        }

        if (!videoPlayer.gameObject.activeSelf)
            videoPlayer.gameObject.SetActive(true);

        videoPlayer.Play();
    }
}
