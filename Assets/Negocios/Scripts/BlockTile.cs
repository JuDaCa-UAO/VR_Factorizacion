using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class BlockTile : MonoBehaviour
{
    [Header("Tag que debe coincidir con el Slot (ej. X2, X, One)")]
    public string blockTag = "X";

    [HideInInspector] public SnapZoneSimple snappedZone; // zona donde está pegado (si la hay)
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        gameObject.tag = blockTag; // por si te olvida setearlo en el inspector
    }

    // Lo llama el SnapZone cuando se pega
    public void AttachToZone(SnapZoneSimple zone)
    {
        snappedZone = zone;
        rb.useGravity = false;
        rb.isKinematic = true;
        transform.position = zone.SnapPoint.position;
        transform.rotation = zone.SnapPoint.rotation;
        transform.SetParent(zone.transform, true);
    }

    // Lo llama el Dragger o la zona cuando se libera
    public void DetachFromZone()
    {
        if (!snappedZone) return;
        transform.SetParent(null, true);
        rb.isKinematic = false;
        rb.useGravity = true;
        snappedZone = null;
    }
}
