using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class TutorialComplete : MonoBehaviour
{
    [Header("Video a observar")]
    [SerializeField] private VideoPlayer videoPlayer;   // El mismo VideoPlayer que usa tu script Audios

    [Header("Canvas final")]
    [SerializeField] private GameObject finalCanvas;    // Canvas con el botón final

    [Header("Escena siguiente")]
    [SerializeField] private string nextSceneName;      // Nombre de la escena a cargar

    [SerializeField] private string endSceneName;

    [Header("Tiempo de espera tras el video")]
    [SerializeField] private float delayAfterVideo = 5f;

    private bool alreadyTriggered = false;

    private void Start()
    {
        if (finalCanvas != null)
            finalCanvas.SetActive(false);  // Aseguramos que el canvas está oculto al inicio

        if (videoPlayer == null)
        {
            Debug.LogError("[FinalCanvasAfterVideo] No se asignó VideoPlayer.");
            return;
        }

        // Nos suscribimos al evento que se dispara cuando el video termina
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    // Este método se llama automáticamente cuando el VideoPlayer termina de reproducir el clip
    private void OnVideoFinished(VideoPlayer vp)
    {
        if (alreadyTriggered) return;
        alreadyTriggered = true;

        // Arrancamos la corrutina que espera X segundos y luego muestra el canvas
        StartCoroutine(ShowCanvasAfterDelay());
    }

    private System.Collections.IEnumerator ShowCanvasAfterDelay()
    {
        // Esperar los 5 segundos (o el valor que pongas en delayAfterVideo)
        yield return new WaitForSeconds(delayAfterVideo);

        if (finalCanvas != null)
        {
            finalCanvas.SetActive(true);
            Debug.Log("[FinalCanvasAfterVideo] Canvas final activado.");
        }
        else
        {
            Debug.LogWarning("[FinalCanvasAfterVideo] No se asignó finalCanvas.");
        }
    }

    // Este método lo llama el botón del Canvas para cambiar de escena
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[FinalCanvasAfterVideo] nextSceneName está vacío, asigna un nombre de escena.");
        }
    }

    public void End()
    {
        if (!string.IsNullOrEmpty(endSceneName))
        {
            SceneManager.LoadScene(endSceneName);
        }
        else
        {
            Debug.LogWarning("[FinalCanvasAfterVideo] nextSceneName está vacío, asigna un nombre de escena.");
        }
    }
}
