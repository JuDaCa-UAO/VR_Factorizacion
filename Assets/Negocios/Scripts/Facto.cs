using UnityEngine;

public class Facto : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab 3D que se instancia al inicio.")]
    public GameObject prefabInicial;

    [Tooltip("Prefab 3D que reemplaza al inicial cuando bloquesCompletos = true.")]
    public GameObject prefabFinal;

    [Header("Transform (EDITA MANUAL)")]
    public Vector3 worldPosition = Vector3.zero;
    public Vector3 worldRotationEuler = Vector3.zero;
    public Vector3 worldScale = Vector3.one;

    [Header("Validación (GameManager)")]
    [Tooltip("Arrastra aquí el GameObject que tiene GestorValidacionGlobal.")]
    public GameObject gameManager;

    [Header("Opciones")]
    public bool instanciarEnAwake = true;   // instancia apenas carga la escena
    public bool liveEditInPlay = true;      // permite mover/rotar/escalar en vivo
    public bool logsDiagnostico = true;     // logs cuando cambie el estado

    // ---- Internos ----
    private GestorValidacionGlobal _gestor;
    private GameObject _instanciaActual;
    private bool _yaReemplazo = false;
    private bool? _ultimoEstado = null;     // para loguear solo cuando cambie

    // ======================================================
    private void Awake()
    {
        if (gameManager != null)
            _gestor = gameManager.GetComponent<GestorValidacionGlobal>();

        if (_gestor == null)
            Debug.LogWarning("⚠️ [Facto] No se encontró GestorValidacionGlobal en el GameObject asignado.");

        if (instanciarEnAwake)
            InstanciarInicial();
    }

    private void Start()
    {
        if (!instanciarEnAwake)
            InstanciarInicial();
    }

    private void Update()
    {
        // Edición en vivo de transform
        if (Application.isPlaying && liveEditInPlay && _instanciaActual != null)
            AplicarTransform(_instanciaActual);

        if (_yaReemplazo || _gestor == null) return;

        // Diagnóstico: log solo cuando cambie el estado
        if (logsDiagnostico)
        {
            if (_ultimoEstado != _gestor.bloquesCompletos)
            {
                _ultimoEstado = _gestor.bloquesCompletos;
                Debug.Log($"[Facto] bloquesCompletos = {_gestor.bloquesCompletos}");
            }
        }

        if (_gestor.bloquesCompletos)
        {
            ReemplazarPorFinal();
            _yaReemplazo = true;
        }
    }

    // ======================================================
    private void InstanciarInicial()
    {
        if (_instanciaActual != null || prefabInicial == null) return;

        _instanciaActual = Instantiate(prefabInicial, worldPosition, Quaternion.Euler(worldRotationEuler));
        _instanciaActual.transform.localScale = worldScale;
        // Debug.Log("[Facto] Prefab inicial instanciado.");
    }

    private void ReemplazarPorFinal()
    {
        if (prefabFinal == null)
        {
            Debug.LogWarning("⚠️ [Facto] PrefabFinal no asignado, no se realiza reemplazo.");
            return;
        }

        if (_instanciaActual != null)
        {
            Destroy(_instanciaActual);
            _instanciaActual = null;
        }

        _instanciaActual = Instantiate(prefabFinal, worldPosition, Quaternion.Euler(worldRotationEuler));
        _instanciaActual.transform.localScale = worldScale;

        if (logsDiagnostico)
            Debug.Log("[Facto] Cambio a prefabFinal realizado.");
    }

    private void AplicarTransform(GameObject go)
    {
        go.transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(worldRotationEuler));
        go.transform.localScale = worldScale;
    }

    // ======================================================
    // 👉 Útil para forzar una re-evaluación desde un botón en el editor
    public void ForceCheck()
    {
        if (_gestor == null)
        {
            Debug.LogWarning("⚠️ [Facto] No hay GestorValidacionGlobal asignado.");
            return;
        }

        if (!_yaReemplazo && _gestor.bloquesCompletos)
        {
            ReemplazarPorFinal();
            _yaReemplazo = true;
        }
        else
        {
            Debug.Log($"[Facto] ForceCheck: bloquesCompletos = {_gestor.bloquesCompletos}, yaReemplazo = {_yaReemplazo}");
        }
    }
}
