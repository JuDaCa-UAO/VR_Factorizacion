using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class dialogosFC : MonoBehaviour
{
    private enum Mode { Main, Hint, Closure }

    [Header("UI")]
    public TextMeshProUGUI paragraphText;

    [Header("Fuente de audio")]
    public audiosFC audioController;   // referencia al script Audios
    public AudioSource audioSource;  // si lo dejas vacío, se toma del controlador

    [Header("Párrafos PRINCIPALES (orden = playlist)")]
    [TextArea(2, 6)]
    public List<string> paragraphs = new List<string>();

    [Header("Párrafos de PISTAS (orden = hintPlaylist)")]
    [TextArea(2, 6)]
    public List<string> hintParagraphs = new List<string>();

    [Header("Párrafo de CIERRE (se muestra cuando suena audio4)")]
    [TextArea(2, 6)]
    public string closureParagraph = "";

    // Estado
    private bool independentMode = false; // true cuando el audio está desactivado
    private Mode currentMode = Mode.Main;
    private int currentParagraphIndex = 0; // índice para main/hint
    private float paragraphElapsed = 0f;

    // Seguimiento
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
            Debug.LogWarning("[Dialogue] Falta referencia a Audios.");

        if (audioSource == null && audioController != null)
            audioSource = audioController.GetComponent<AudioSource>();

        // Inicial: primer párrafo principal si existe
        currentMode = Mode.Main;
        currentParagraphIndex = 0;
        ApplyParagraph(currentMode, currentParagraphIndex, resetElapsed: true);
    }

    private void Update()
    {
        if (audioSource == null || audioController == null || paragraphText == null)
        {
            RunIndependentTimer(); // fallback
            return;
        }

        bool isPlaying = audioSource.isPlaying;
        AudioClip currentClip = audioSource.clip;

        // Detección de modo según el clip actual
        Mode detectedMode = DetectMode(currentClip, out int idxMain, out int idxHint);

        bool clipChanged = (currentClip != lastClip) || (detectedMode != currentMode);

        if (!independentMode)
        {
            if (clipChanged && currentClip != null)
            {
                currentMode = detectedMode;
                if (currentMode == Mode.Main)
                    currentParagraphIndex = Mathf.Clamp(idxMain, 0, Mathf.Max(0, paragraphs.Count - 1));
                else if (currentMode == Mode.Hint)
                    currentParagraphIndex = Mathf.Clamp(idxHint, 0, Mathf.Max(0, hintParagraphs.Count - 1));
                else // Closure
                    currentParagraphIndex = 0; // solo un texto de cierre

                ApplyParagraph(currentMode, currentParagraphIndex, resetElapsed: true);
            }

            // Mientras suena, alinear tiempo del párrafo al tiempo del audio
            if (isPlaying && currentClip != null)
            {
                paragraphElapsed = Mathf.Clamp(audioSource.time, 0f, GetDuration(currentMode, currentParagraphIndex));
            }

            // Caso especial: una pista terminó naturalmente (dejó de sonar sin cambio de clip)
            // → limpiar en el acto (no queremos timers ni avanzar a otra pista)
            if (currentMode == Mode.Hint && !isPlaying && lastIsPlaying && !clipChanged && currentClip != null)
            {
                ClearParagraph();
            }
        }

        // Si se detiene sin cambio de clip → probablemente DESACTIVASTE el audio (modo independiente)
        if (!isPlaying && lastIsPlaying && !clipChanged && currentClip != null)
        {
            // Solo entrar a independiente si fue una pausa/stop manual.
            // Si era hint y terminó natural, ya limpiamos arriba; no activar independiente.
            if (!(currentMode == Mode.Hint && Mathf.Abs(lastKnownAudioTime - GetDuration(currentMode, currentParagraphIndex)) < 0.05f))
            {
                independentMode = true;
                paragraphElapsed = Mathf.Clamp(lastKnownAudioTime, 0f, GetDuration(currentMode, currentParagraphIndex));
            }
        }

        // Si vuelve a activarse el audio → realinear
        if (isPlaying && !lastIsPlaying && currentClip != null)
        {
            // recalcular modo/índice por seguridad
            detectedMode = DetectMode(currentClip, out idxMain, out idxHint);
            currentMode = detectedMode;
            if (currentMode == Mode.Main)
                currentParagraphIndex = Mathf.Clamp(idxMain, 0, Mathf.Max(0, paragraphs.Count - 1));
            else if (currentMode == Mode.Hint)
                currentParagraphIndex = Mathf.Clamp(idxHint, 0, Mathf.Max(0, hintParagraphs.Count - 1));
            else
                currentParagraphIndex = 0;

            ApplyParagraph(currentMode, currentParagraphIndex, resetElapsed: true);
            independentMode = false;
            paragraphElapsed = audioSource.time;
        }

        if (independentMode) RunIndependentTimer();

        // Guardar estado
        lastIsPlaying = isPlaying;
        lastClip = currentClip;
        if (isPlaying && currentClip != null) lastKnownAudioTime = audioSource.time;
    }

    // ---------- Temporizador independiente ----------
    private void RunIndependentTimer()
    {
        float dur = GetDuration(currentMode, currentParagraphIndex);
        paragraphElapsed += Time.deltaTime;

        if (paragraphElapsed >= dur)
        {
            // Comportamiento por modo mientras el audio está desactivado:
            if (currentMode == Mode.Main)
            {
                // Igual que antes: avanzar por los párrafos principales
                int next = currentParagraphIndex + 1;
                if (next < paragraphs.Count)
                {
                    currentParagraphIndex = next;
                    ApplyParagraph(Mode.Main, currentParagraphIndex, resetElapsed: true);
                }
                else
                {
                    ClearParagraph();
                }
            }
            else if (currentMode == Mode.Hint)
            {
                // Para pistas: NO avanzar; solo limpiar al terminar la duración
                ClearParagraph();
            }
            else // Closure
            {
                // Cierre: dura lo mismo que audio4 y luego limpia
                ClearParagraph();
            }
        }
    }

    // ---------- Utilidades ----------
    private void ApplyParagraph(Mode mode, int index, bool resetElapsed)
    {
        if (resetElapsed) paragraphElapsed = 0f;
        if (paragraphText == null) return;

        switch (mode)
        {
            case Mode.Main:
                if (paragraphs.Count == 0) { paragraphText.text = ""; return; }
                index = Mathf.Clamp(index, 0, paragraphs.Count - 1);
                paragraphText.text = paragraphs[index];
                break;

            case Mode.Hint:
                if (hintParagraphs.Count == 0) { paragraphText.text = ""; return; }
                index = Mathf.Clamp(index, 0, hintParagraphs.Count - 1);
                paragraphText.text = hintParagraphs[index];
                break;

            case Mode.Closure:
                paragraphText.text = closureParagraph ?? "";
                break;
        }
    }

    private void ClearParagraph()
    {
        if (paragraphText != null) paragraphText.text = "";
    }

    private float GetDuration(Mode mode, int index)
    {
        if (audioController == null) return 3f;

        if (mode == Mode.Main)
        {
            if (audioController.playlist == null || index < 0 || index >= audioController.playlist.Count) return 3f;
            var c = audioController.playlist[index];
            return (c != null && c.length > 0f) ? c.length : 3f;
        }
        else if (mode == Mode.Hint)
        {
            if (audioController.hintPlaylist == null || index < 0 || index >= audioController.hintPlaylist.Count) return 3f;
            var c = audioController.hintPlaylist[index];
            return (c != null && c.length > 0f) ? c.length : 3f;
        }
        else // Closure
        {
            var c = audioController.audio4;
            return (c != null && c.length > 0f) ? c.length : 3f;
        }
    }

    private Mode DetectMode(AudioClip clip, out int idxMain, out int idxHint)
    {
        idxMain = -1; idxHint = -1;
        if (clip == null || audioController == null) return currentMode;

        // ¿es cierre?
        if (audioController.audio4 != null && clip == audioController.audio4)
            return Mode.Closure;

        // ¿es principal?
        idxMain = IndexOfClip(audioController.playlist, clip);
        if (idxMain != -1) return Mode.Main;

        // ¿es pista?
        idxHint = IndexOfClip(audioController.hintPlaylist, clip);
        if (idxHint != -1) return Mode.Hint;

        // si no lo encuentra, mantén el modo actual
        return currentMode;
    }

    private int IndexOfClip(List<AudioClip> list, AudioClip clip)
    {
        if (list == null || clip == null) return -1;
        return list.IndexOf(clip);
    }
}
