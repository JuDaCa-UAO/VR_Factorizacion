using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AudioFinalTutorial : MonoBehaviour
{
    [Header("Audio final de recompensa")]
    public AudioClip audioFinal;            // asigna el clip
    [TextArea] public string mensajeFinal = "¡Felicidades, completaste el tutorial!";

    [Header("UI")]
    public TextMeshProUGUI textoUI;         // arrastra el TMP (opcional)

    [Header("Salida en vídeo")]
    [Tooltip("RawImage que mostrará el video (debe iniciar desactivado).")]
    public RawImage rewardRawImage;         // arrastra el RawImage
    public VideoPlayer videoPlayer;         // arrastra el VideoPlayer

    [Header("Escena siguiente")]
    public string nombreEscenaSiguiente = "siguiente";
    public float delayDespuesDeVideo = 5f;

    private AudioSource _as;
    private bool _corriendo = false;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
        _as.playOnAwake = false;
        _as.loop = false;

        // Deja todo limpio de entrada
        ApagarVideoUI();
    }

    /// <summary>
    /// Método de utilidad para dejar el estado visual limpio.
    /// Llamado por el activador en Start, por si quedó algo encendido en el editor.
    /// </summary>
    public void ResetState()
    {
        StopAllCoroutines();
        _corriendo = false;
        _as.Stop();
        ApagarVideoUI();
    }

    private void ApagarVideoUI()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.gameObject.SetActive(false);
        }
        if (rewardRawImage != null)
            rewardRawImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Arranca la secuencia (audio -> rawimage+video -> cambio de escena).
    /// </summary>
    public void IniciarSecuencia()
    {
        if (_corriendo) return;
        StartCoroutine(SecuenciaFinalCoroutine());
    }

    private IEnumerator SecuenciaFinalCoroutine()
    {
        _corriendo = true;

        // Texto de felicitación (si hay)
        if (textoUI != null)
            textoUI.text = mensajeFinal;

        // 1) Audio final
        if (audioFinal != null)
        {
            _as.clip = audioFinal;
            _as.Play();
            yield return new WaitForSeconds(audioFinal.length);
        }

        // 2) Mostrar RawImage + Video
        if (rewardRawImage != null)
            rewardRawImage.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(true);

            // Si el VideoPlayer usa RenderTexture, basta con activarlo y darle Play.
            videoPlayer.Play();

            // Esperar a que termine (loopPointReached o isPlaying false)
            // Más robusto: espera a que empiece
            while (!videoPlayer.isPlaying) { yield return null; }
            // Ahora espera a que acabe
            while (videoPlayer.isPlaying) { yield return null; }
        }

        // 3) Ocultar RawImage
        if (rewardRawImage != null)
            rewardRawImage.gameObject.SetActive(false);
        if (videoPlayer != null)
            videoPlayer.gameObject.SetActive(false);

        // 4) Espera y cambio de escena
        yield return new WaitForSeconds(delayDespuesDeVideo);

        if (!string.IsNullOrEmpty(nombreEscenaSiguiente))
        {
            SceneManager.LoadScene(nombreEscenaSiguiente);
        }
        else
        {
            Debug.LogWarning("⚠️ [AudioFinalTutorial] No se asignó 'Nombre Escena Siguiente'.");
        }

        _corriendo = false;
    }
}
