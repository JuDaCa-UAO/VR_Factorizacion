using UnityEngine;
using UnityEngine.InputSystem; // Input System (Mouse)

public class BlockDragger : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] LayerMask blockMask = ~0; // filtra si usas la capa "Blocks"
    [SerializeField] float followSpeed = 20f;   // suavidad al seguir el mouse
    [SerializeField] float liftWhileDrag = 0.05f; // pequeño “levantamiento” visual

    Rigidbody held;
    Plane dragPlane;
    float fixedY;              // altura a la que “planea” mientras arrastras
    Vector3 grabOffset;        // punto de agarre relativo al centro del rigidbody
    float origDrag, origAngDrag;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

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
        if (Physics.Raycast(ray, out var hit, 1000f, blockMask))
        {
            var rb = hit.rigidbody ?? hit.collider.attachedRigidbody;
            if (rb == null) return;

            held = rb;

            // Guardar estado original y “preparar” el rigidbody
            origDrag = held.drag; origAngDrag = held.angularDrag;
            held.useGravity = false;     // que no caiga mientras lo mueves
            held.drag = 20f;             // amortigua movimientos bruscos
            held.angularDrag = 20f;

            // Plano de arrastre “a la altura del bloque”
            fixedY = held.position.y;
            dragPlane = new Plane(Vector3.up, new Vector3(0f, fixedY, 0f));

            // Offset desde el centro del rb al punto donde hiciste click
            grabOffset = hit.point - held.worldCenterOfMass;
        }
    }

    void Drag()
    {
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (dragPlane.Raycast(ray, out float enter))
        {
            var hitPoint = ray.GetPoint(enter);

            // Mantén la misma altura (fixedY) y respeta el punto de agarre
            var target = hitPoint - grabOffset;
            target.y = fixedY + liftWhileDrag;

            // Movimiento suave y “físico”
            var next = Vector3.Lerp(held.position, target, Time.deltaTime * followSpeed);
            held.MovePosition(next);
        }
    }

    void Drop()
    {
        if (held == null) return;

        // Restituir estado del rigidbody
        held.useGravity = true;
        held.drag = origDrag;
        held.angularDrag = origAngDrag;

        held = null;
    }
}
