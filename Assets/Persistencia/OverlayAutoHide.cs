using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
public class OverlayAutoHide : MonoBehaviour
{
    [Header("Referencias UI")]
    public Canvas canvas;                 // debe ser Screen Space - Overlay
    public CanvasGroup canvasGroup;       // para fade in/out
    public Image backgroundPanel;         // panel negro semitransparente (opcional)
    public TextMeshProUGUI countdownTMP;  // opcional: contador mm:ss

    [Header("Animación")]
    public float fadeInSeconds = 0.25f;
    public float fadeOutSeconds = 0.25f;

    [Header("Texto")]
    public string countdownPrefix = "Cerrando en: ";

    private void Reset()
    {
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (!canvas) canvas = GetComponent<Canvas>();
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (backgroundPanel != null) backgroundPanel.raycastTarget = true;
        gameObject.SetActive(false); // empieza oculto
    }

    /// <summary>
    /// Muestra el overlay por 'visibleSeconds' con fade y lo desactiva.
    /// Devuelve una corrutina que termina cuando ya se escondió.
    /// </summary>
    public IEnumerator ShowAndWait(float visibleSeconds)
    {
        // Activar GO por si estaba desactivado
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Fade in
        yield return FadeTo(1f, fadeInSeconds);

        // Contador mientras está visible
        float remaining = Mathf.Max(0f, visibleSeconds);
        while (remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            UpdateCountdown(Mathf.Max(0f, remaining));
            yield return null;
        }

        // Fade out
        yield return FadeTo(0f, fadeOutSeconds);

        // Desactivar GO
        gameObject.SetActive(false);
    }

    public void HideImmediate()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = target;
            yield break;
        }

        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        canvasGroup.alpha = target;
    }

    private void UpdateCountdown(float seconds)
    {
        if (countdownTMP == null) return;

        int total = Mathf.CeilToInt(seconds);
        int mm = total / 60;
        int ss = total % 60;
        countdownTMP.text = $"{countdownPrefix}{mm:00}:{ss:00}";
    }
}
