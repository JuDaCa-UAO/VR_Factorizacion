using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI paragraphText;

    [Header("Fuente de audio (usa tu controlador anterior)")]
    public Audios audioController; // referencia al script anterior
    public AudioSource audioSource;                   // si lo dejas vacío, lo toma del controlador

    [Header("Párrafos (mismo orden que los audios)")]
    [TextArea(2, 6)]
    public List<string> paragraphs = new List<string>();

    // Duraciones calculadas desde los AudioClips
    private List<float> paragraphDurations = new List<float>();

    // Estado interno
    private int currentParagraphIndex = 0;
    private float paragraphElapsed = 0f;
    private bool independentMode = false; // true cuando el audio está desactivado

    // Seguimiento para detectar eventos
    private AudioClip lastClip = null;
    private bool lastIsPlaying = false;
    private float lastKnownAudioTime = 0f;

    private void Reset()
    {
        paragraphText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (audioController == null)
        {
            Debug.LogWarning("[VRParagraphDisplayController] Falta referencia a VRPlaylistAudioController.");
        }

        if (audioSource == null && audioController != null)
        {
            audioSource = audioController.GetComponent<AudioSource>();
        }

        BuildDurationsFromPlaylist();
        ClampIndices();
        ApplyParagraph(currentParagraphIndex, resetElapsed: true);
    }

    private void Update()
    {
        if (audioSource == null || audioController == null || paragraphText == null)
        {
            // En caso de no tener referencias, al menos avanza independiente
            RunIndependentTimer();
            return;
        }

        bool isPlaying = audioSource.isPlaying;
        AudioClip currentClip = audioSource.clip;

        // 1) Detectar cambio de clip (se avanzó al siguiente audio por reproducción)
        int clipIndex = IndexOfClipInController(currentClip);
        int lastClipIndex = IndexOfClipInController(lastClip);

        bool clipChanged = (clipIndex != -1 && clipIndex != lastClipIndex);

        if (!independentMode)
        {
            // Modo sincronizado con el audio
            if (clipChanged)
            {
                // Se cambió de clip por secuencia
                currentParagraphIndex = Mathf.Clamp(clipIndex, 0, paragraphs.Count - 1);
                ApplyParagraph(currentParagraphIndex, resetElapsed: true);
            }

            // Si el audio está reproduciendo, alineamos el tiempo del párrafo al tiempo del audio
            if (isPlaying && currentClip != null)
            {
                paragraphElapsed = Mathf.Clamp(audioSource.time, 0f, GetDuration(currentParagraphIndex));
                // Si por cualquier motivo se alcanzó el final, dejamos que el audioController maneje el salto;
                // nosotros nos realineamos en el próximo frame por el cambio de clip.
            }
        }

        // 2) Detectar desactivación/pausa global (stop manual sin cambio de clip)
        // Transición: estaba reproduciendo y ahora no, y el clip NO cambió -> probable desactivación
        if (!isPlaying && lastIsPlaying && !clipChanged && currentClip != null)
        {
            // si llegamos al último párrafo y ya cumplió su tiempo, limpiar texto
            if (currentParagraphIndex == paragraphs.Count - 1 &&
                lastKnownAudioTime >= GetDuration(currentParagraphIndex) - 0.001f)
            {
                ClearParagraph();
            }

            // Entra a modo independiente desde el punto donde iba
            independentMode = true;
            paragraphElapsed = Mathf.Clamp(lastKnownAudioTime, 0f, GetDuration(currentParagraphIndex));
        }


        // 3) Si se vuelve a activar (audio comienza a reproducir)
        if (isPlaying && !lastIsPlaying && currentClip != null)
        {
            // Realinear al clip actual y salir de independiente
            int nowIndex = IndexOfClipInController(currentClip);
            if (nowIndex != -1)
            {
                currentParagraphIndex = Mathf.Clamp(nowIndex, 0, paragraphs.Count - 1);
                ApplyParagraph(currentParagraphIndex, resetElapsed: true);
            }
            independentMode = false;
            paragraphElapsed = audioSource.time;
        }

        // 4) Ejecutar el temporizador independiente si corresponde
        if (independentMode)
        {
            RunIndependentTimer();
        }

        // Guardar estado para el siguiente frame
        lastIsPlaying = isPlaying;
        lastClip = currentClip;

        // Mientras suena, guardamos el tiempo para poder retomarlo si se desactiva
        if (isPlaying && currentClip != null)
        {
            lastKnownAudioTime = audioSource.time;
        }
    }


    // Temporizador independiente
  
    private void RunIndependentTimer()
    {
        if (paragraphs.Count == 0) return;

        paragraphElapsed += Time.deltaTime;
        float dur = GetDuration(currentParagraphIndex);

        if (paragraphElapsed >= dur)
        {
            // Pasar al siguiente párrafo aunque el audio esté desactivado
            int next = currentParagraphIndex + 1;
            if (next < paragraphs.Count)
            {
                currentParagraphIndex = next;
                ApplyParagraph(currentParagraphIndex, resetElapsed: true);
            }
            else
            {
                // Último párrafo completado: limpiar texto
                ClearParagraph();
            }
        }
    }


    // Utilidades

    private void ApplyParagraph(int index, bool resetElapsed)
    {
        if (paragraphs.Count == 0) return;

        index = Mathf.Clamp(index, 0, paragraphs.Count - 1);
        if (resetElapsed) paragraphElapsed = 0f;

        if (paragraphText != null)
        {
            paragraphText.text = paragraphs[index];
        }
    }

    private void BuildDurationsFromPlaylist()
    {
        paragraphDurations.Clear();

        int n = paragraphs.Count;
        for (int i = 0; i < n; i++)
        {
            float dur = 3f; // fallback si no hay clip
            if (audioController != null && audioController.playlist != null && i < audioController.playlist.Count && audioController.playlist[i] != null)
            {
                dur = Mathf.Max(0.1f, audioController.playlist[i].length);
            }
            paragraphDurations.Add(dur);
        }
    }

    private void ClearParagraph()
    {
        if (paragraphText != null) paragraphText.text = "";
    }


    private float GetDuration(int index)
    {
        if (index < 0 || index >= paragraphDurations.Count) return 3f;
        return Mathf.Max(0.1f, paragraphDurations[index]);
    }

    private int IndexOfClipInController(AudioClip clip)
    {
        if (clip == null || audioController == null || audioController.playlist == null) return -1;
        return audioController.playlist.IndexOf(clip);
        // Nota: depende de que "playlist" del controlador sea pública (lo es en tu script).
    }

    private void ClampIndices()
    {
        if (paragraphs.Count == 0) return;
        currentParagraphIndex = Mathf.Clamp(currentParagraphIndex, 0, paragraphs.Count - 1);
    }

    // Si cambias los párrafos en el inspector en tiempo de edición,
    // intenta reconstruir duraciones automáticamente
#if UNITY_EDITOR
    private void OnValidate()
    {
        // reconstruir duraciones si cambia el tamaño de paragraphs
        if (paragraphDurations == null) paragraphDurations = new List<float>();
        if (paragraphs == null) paragraphs = new List<string>();

        // Ajusta el tamaño para coincidir
        if (paragraphDurations.Count != paragraphs.Count)
        {
            BuildDurationsFromPlaylist();
        }
    }
#endif
}
