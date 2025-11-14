using System.Collections;
using UnityEngine;

public class IntervalCanvasManager : MonoBehaviour
{
    public static IntervalCanvasManager Instance { get; private set; }

    [Header("Intervalo entre avisos")]
    [Min(0f)] public float intervalSeconds = 300f;  // tiempo de espera antes de mostrar el canvas (editable)
    public bool autoStart = true;
    public bool persistAcrossScenes = true;

    [Header("Prefab del Canvas")]
    [Tooltip("Prefab con OverlayAutoHide en un Canvas (Screen Space - Overlay).")]
    public GameObject canvasPrefab;

    [Header("Duración visible del canvas")]
    [Min(0f)] public float canvasVisibleSeconds = 6f;  // cuánto tiempo permanece visible cada vez

    private GameObject _canvasInstance;
    private OverlayAutoHide _overlay;
    private Coroutine _loop;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (autoStart) StartLoop();
    }

    public void StartLoop()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = StartCoroutine(LoopRoutine());
    }

    public void StopLoop()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
    }

    private IEnumerator LoopRoutine()
    {
        while (true)
        {
            // 1) Esperar el intervalo (independiente de timeScale)
            float t = intervalSeconds;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                yield return null;
            }

            // 2) Asegurar instancia del canvas
            EnsureCanvas();

            // 3) Mostrar el canvas y esperar a que termine su propio ciclo
            if (_overlay != null)
            {
                // ShowAndWait se encarga de activar, mostrar el contador, hacer fade y desactivar el GO
                yield return _overlay.ShowAndWait(canvasVisibleSeconds);
            }
            else
            {
                // Fallback simple por si el prefab no tiene OverlayAutoHide
                _canvasInstance.SetActive(true);
                yield return new WaitForSecondsRealtime(canvasVisibleSeconds);
                _canvasInstance.SetActive(false);
            }

            // 4) Repetir
        }
    }

    private void EnsureCanvas()
    {
        if (canvasPrefab == null)
        {
            Debug.LogWarning("[IntervalCanvasManager] No hay canvasPrefab asignado.");
            return;
        }

        if (_canvasInstance == null)
        {
            _canvasInstance = Instantiate(canvasPrefab);
            if (persistAcrossScenes) DontDestroyOnLoad(_canvasInstance);
            _overlay = _canvasInstance.GetComponent<OverlayAutoHide>();
            if (_overlay == null)
                Debug.LogWarning("[IntervalCanvasManager] El prefab no tiene OverlayAutoHide.");
        }
    }
}
