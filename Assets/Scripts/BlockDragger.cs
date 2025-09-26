using UnityEngine;
using UnityEngine.InputSystem;

public class BlockDragger : MonoBehaviour
{
    [SerializeField] Camera cam;

    [Header("Layers")]
    [SerializeField] LayerMask blockLayer; // capa de los bloques (p.ej. "Blocks")

    [Header("Movimiento")]
    [SerializeField] float followSpeed = 20f;
    [SerializeField] float magnetStrength = 10f; // fuerza del imán hacia el SnapPoint

    Rigidbody held;
    GameObject heldGO;
    Plane dragPlane;                 // plano a la altura del bloque
    SnapZoneSimple hoverZone = null; // slot hacia el que nos “imanta”

    void Awake() { if (!cam) cam = Camera.main; }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame) TryPick();
        if (Mouse.current.leftButton.isPressed && held) Drag();
        if (Mouse.current.leftButton.wasReleasedThisFrame) Drop();
    }

    void TryPick()
    {
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out var hit, 1000f, blockLayer, QueryTriggerInteraction.Ignore)) return;

        var rb = hit.rigidbody ?? hit.collider.attachedRigidbody;
        if (!rb) return;

        // si ya está pegado, no permitir tomarlo
        var st = rb.GetComponent<BlockSnapState>();
        if (st && st.isSnapped) return;

        held = rb;
        heldGO = rb.gameObject;

        dragPlane = new Plane(Vector3.up, held.position);

        held.isKinematic = true;
        held.useGravity = false;
    }

    void Drag()
    {
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!dragPlane.Raycast(ray, out float enter)) return;

        var hitPoint = ray.GetPoint(enter);
        var target = hitPoint; target.y = held.position.y;

        // Buscar slot más cercano por distancia al SnapPoint
        SnapZoneSimple best = null; float bestD = float.MaxValue;
        var zones = Object.FindObjectsByType<SnapZoneSimple>(FindObjectsSortMode.None);
        foreach (var z in zones)
        {
            float d = z.DistanceTo(held.position);
            if (d < bestD) { bestD = d; best = z; }
        }

        // Si estamos dentro del radio del mejor slot compatible → “imán”
        if (best && best.Matches(heldGO) && bestD <= best.captureRadius)
        {
            var sp = best.SnapPoint.position; sp.y = held.position.y;
            target = Vector3.Lerp(target, sp, Time.deltaTime * magnetStrength);
            hoverZone = best;
        }
        else
        {
            hoverZone = null;
        }

        var next = Vector3.Lerp(held.position, target, Time.deltaTime * followSpeed);
        held.MovePosition(next);
    }

    void Drop()
    {
        if (!held) return;

        // Si teníamos una zona “magnética” válida, pegamos sin exigir precisión
        if (hoverZone && hoverZone.Matches(heldGO))
        {
            hoverZone.Snap(held); // fija kinematic + parent + marca estado
        }
        else
        {
            held.isKinematic = false;
            held.useGravity = true;
            held.transform.SetParent(null, true);
        }

        held = null;
        heldGO = null;
        hoverZone = null;
    }
}
