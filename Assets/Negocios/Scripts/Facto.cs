using UnityEngine;

public class Facto : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefabInicial;          // Se muestra al inicio
    public GameObject prefabFinal;            // Reemplaza cuando bloquesCompletos = true

    [Header("Transform (EDITA EN VIVO)")]
    public Vector3 worldPosition = Vector3.zero;
    public Vector3 worldRotationEuler = Vector3.zero;
    public Vector3 worldScale = Vector3.one;

    [Header("Live Edit")]
    [Tooltip("Si está activo, durante el Play la instancia seguirá estos valores cada frame.")]
    public bool liveEditInPlay = true;

    [Header("Validación (GameManager)")]
    [Tooltip("GameObject que tiene GestorValidacionGlobal.")]
    public GameObject gameManager;

    [Header("Opciones")]
    public bool instanciarEnAwake = true;     // Instancia apenas carga
    public Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.6f);
    public float gizmoSize = 0.2f;

    // ---- Internos ----
    private GestorValidacionGlobal _validador;
    private GameObject _instanciaActual;
    private bool _reemplazado = false;

    private void Awake()
    {
        if (gameManager != null)
            _validador = gameManager.GetComponent<GestorValidacionGlobal>();

        if (_validador == null)
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
        // Edita en vivo durante el Play
        if (Application.isPlaying && liveEditInPlay && _instanciaActual != null)
            AplicarTransformEnInstancia(_instanciaActual);

        // Swap cuando el gestor diga que todo está correcto
        if (!_reemplazado && _validador != null && _validador.bloquesCompletos)
            ReemplazarPorFinal();
    }

    private void InstanciarInicial()
    {
        if (_instanciaActual != null || prefabInicial == null) return;

        _instanciaActual = Instantiate(prefabInicial, worldPosition, Quaternion.Euler(worldRotationEuler));
        _instanciaActual.transform.localScale = worldScale;
        // Debug.Log("[Facto] Prefab inicial instanciado.");
    }

    private void ReemplazarPorFinal()
    {
        _reemplazado = true;

        if (prefabFinal == null)
        {
            Debug.LogWarning("⚠️ [Facto] PrefabFinal no asignado. No se realiza el reemplazo.");
            return;
        }

        if (_instanciaActual != null)
        {
            Destroy(_instanciaActual);
            _instanciaActual = null;
        }

        _instanciaActual = Instantiate(prefabFinal, worldPosition, Quaternion.Euler(worldRotationEuler));
        _instanciaActual.transform.localScale = worldScale;
        // Debug.Log("[Facto] Prefab final instanciado.");
    }

    private void AplicarTransformEnInstancia(GameObject go)
    {
        go.transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(worldRotationEuler));
        go.transform.localScale = worldScale;
    }

    // Botón útil para re-aplicar transform manualmente (desde el inspector con scripts que soporten buttons)
    public void ReaplicarTransform()
    {
        if (_instanciaActual != null)
            AplicarTransformEnInstancia(_instanciaActual);
    }

    // Gizmos para visualizar el punto/rotación en editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(worldPosition, gizmoSize);

        var rot = Quaternion.Euler(worldRotationEuler);
        Gizmos.color = Color.red; Gizmos.DrawLine(worldPosition, worldPosition + rot * Vector3.right * gizmoSize * 3f);
        Gizmos.color = Color.green; Gizmos.DrawLine(worldPosition, worldPosition + rot * Vector3.up * gizmoSize * 3f);
        Gizmos.color = Color.blue; Gizmos.DrawLine(worldPosition, worldPosition + rot * Vector3.forward * gizmoSize * 3f);
    }
}
