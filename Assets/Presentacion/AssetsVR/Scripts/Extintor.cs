using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Extintor : MonoBehaviour
{
    public XRGrabInteractable grabInteractable;
    public Chorro chorro;

    void Start()
    {
        // Escuchar el evento de activación (cuando se presiona el gatillo)
        grabInteractable.activated.AddListener(OnActivated);

        // Escuchar el evento de desactivación (cuando se suelta el gatillo)
        grabInteractable.deactivated.AddListener(OnDeactivated);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        chorro.StartChorrear(); // Inicia el chorro cuando se presiona el gatillo
    }

    private void OnDeactivated(DeactivateEventArgs args)
    {
        chorro.StopChorrear(); // Detiene el chorro cuando se suelta el gatillo
    }
}
