using UnityEngine;

public enum XOrientation { Any, Horizontal, Vertical }

[RequireComponent(typeof(Collider))]
public class SnapZoneSimple : MonoBehaviour
{
    [Header("Coincidencia")]
    [Tooltip("Debe coincidir con el Tag del bloque (X2, X, One, etc.)")]
    public string requiredTag = "X";

    [Header("Captura")]
    [Tooltip("Distancia maxima (m) a la que el slot 'captura' el bloque")]
    public float captureRadius = 0.6f;

    [Header("Orientacion para piezas X")]
    public XOrientation xOrientation = XOrientation.Any;  // Horizontal = X×1, Vertical = 1×X
    public bool rotateXOnSnap = true;                     // forzar rotacion al pegar

    [Tooltip("Si true, la rotacion final = SnapPoint.rotation * preset; si false, solo el preset.")]
    public bool xRotationRelativeToSnapPoint = true;

    [Tooltip("Preset de rotacion (en Euler locales del bloque) para X horizontal (X×1).")]
    public Vector3 xHorizontalLocalEuler = new Vector3(0f, 0f, 0f);

    [Tooltip("Preset de rotacion (en Euler locales del bloque) para X vertical (1×X).")]
    public Vector3 xVerticalLocalEuler = new Vector3(0f, 90f, 0f);

    [Header("Rotacion para otras piezas")]
    public bool rotateOthersOnSnap = false;               // X2 y One normalmente no rotan

    [Tooltip("Punto exacto donde se pegara el bloque. Si no se asigna, usa este transform.")]
    public Transform snapPoint;
    public Transform SnapPoint => snapPoint ? snapPoint : transform;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!snapPoint)
        {
            var p = new GameObject("SnapPoint").transform;
            p.SetParent(transform, false);
            p.localPosition = Vector3.zero;
            snapPoint = p;
        }

        int layer = LayerMask.NameToLayer("Slots");
        if (layer != -1) gameObject.layer = layer;
    }

    public bool Matches(GameObject blockGO)
    {
        if (blockGO.CompareTag(requiredTag)) return true;
        foreach (var t in blockGO.GetComponentsInChildren<Transform>(true))
            if (t.CompareTag(requiredTag)) return true;
        return false;
    }

    // Distancia en plano XZ
    public float DistanceTo(Vector3 worldPos)
    {
        var a = SnapPoint.position; a.y = 0f;
        var b = worldPos; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    public void Snap(GameObject blockGO)
    {
        if (!Matches(blockGO)) return;
        var rb = blockGO.GetComponent<Rigidbody>();
        if (!rb) return;

        // Posicion
        rb.isKinematic = true;
        rb.useGravity = false;
        blockGO.transform.position = SnapPoint.position;

        // Rotacion segun tipo
        if (requiredTag == "X" && rotateXOnSnap)
        {
            // Elegimos preset segun la orientacion declarada del slot
            Vector3 presetEuler = xOrientation == XOrientation.Vertical
                                  ? xVerticalLocalEuler
                                  : xHorizontalLocalEuler; // Horizontal por defecto

            Quaternion preset = Quaternion.Euler(presetEuler);

            Quaternion finalRot = xRotationRelativeToSnapPoint
                                  ? SnapPoint.rotation * preset
                                  : preset;

            blockGO.transform.rotation = finalRot;
        }
        else if (requiredTag != "X" && rotateOthersOnSnap)
        {
            blockGO.transform.rotation = SnapPoint.rotation;
        }
        // Si rotateXOnSnap == false o xOrientation == Any y no quieres tocar rotacion,
        // no hacemos nada: conserva la rotacion del bloque.

        blockGO.transform.SetParent(transform, true);

        // Estado pegado
        var st = blockGO.GetComponent<BlockSnapState>();
        if (!st) st = blockGO.AddComponent<BlockSnapState>();
        st.isSnapped = true;
        st.snappedZone = this;
    }

    public void Snap(Rigidbody rb)
    {
        if (!rb) return;
        Snap(rb.gameObject);
    }

    // Gizmos (corregido el matrix)
    void OnDrawGizmos()
    {
        if (TryGetComponent<BoxCollider>(out var c))
        {
            Gizmos.color = new Color(0, 1, 1, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(c.center, c.size);
        }

        // reset
        Gizmos.matrix = Matrix4x4.identity;

        var sp = SnapPoint ? SnapPoint.position : transform.position;

        Gizmos.color = new Color(0, 1, 1, 0.15f);
        Gizmos.DrawWireSphere(sp, captureRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(sp, 0.02f);
    }
}
