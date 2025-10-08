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

    [Header("Sonidos")]
    [SerializeField] Audios audioController; // Referencia a Audios
    [SerializeField] AudioClip correctSound;  // Sonido A: cuando el bloque es colocado correctamente
    [SerializeField] AudioClip incorrectSound; // Sonido B: cuando el bloque no se coloca en el lugar correcto
    [SerializeField] AudioClip allBlocksCorrectSound;
    public AudioSource audioSource;
    private bool hasPlayedCorrectSound = false;


    Rigidbody held;
    GameObject heldGO;
    Plane dragPlane;                 // plano a la altura del bloque
    SnapZoneSimple hoverZone = null; // slot hacia el que nos “imanta”

    void Awake() {
        if (!cam) cam = Camera.main;
        audioSource = GetComponent<AudioSource>(); // Obtener el AudioSource
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame) TryPick();
        if (Mouse.current.leftButton.isPressed && held) Drag();
        if (Mouse.current.leftButton.wasReleasedThisFrame) Drop();

        if (AllBlocksSnapped() && !hasPlayedCorrectSound)
        {
            // Reproducir sonido C cuando todos los bloques estén correctos, solo una vez
            audioSource.PlayOneShot(allBlocksCorrectSound);
            hasPlayedCorrectSound = true;  // Marcar que el sonido se ha reproducido

            // Llamar a la función en Audios para reproducir el nuevo audio y texto
            audioController.CheckAllBlocksSnapped();
        }
    }
    // Verifica si todos los bloques están en su lugar
    public bool AllBlocksSnapped()
    {
        var blocks = Object.FindObjectsByType<BlockSnapState>(FindObjectsSortMode.None);
        foreach (var block in blocks)
        {
            if (!block.isSnapped) return false;  // Si algún bloque no está bien posicionado
        }
        return true;  // Si todos los bloques están bien, retornar true
    }

    // Llamar a VRPlaylistAudioController y VRParagraphDisplayController cuando todos los bloques estén correctos


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
                                  // Reproducir sonido  (correcto)
            audioSource.PlayOneShot(correctSound);
        }
        else
        {
            held.isKinematic = false;
            held.useGravity = true;
            held.transform.SetParent(null, true);
            // Reproducir sonido  (incorrecto)
            audioSource.PlayOneShot(incorrectSound);
        }

        held = null;
        heldGO = null;
        hoverZone = null;
    }
}
