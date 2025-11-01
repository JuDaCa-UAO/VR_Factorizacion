using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class RotadorInteractableManual : MonoBehaviour
{
    [Header("Parámetros")]
    public float velocidadRotacion = 100f;

    [Header("Componente externo")]
    public ContinuousTurnProviderBase turnProvider; // Asigna desde XR Rig

    private XRGrabInteractable grabInteractable;
    private Transform interactorTransform;

    private Quaternion rotacionInicialObjeto;
    private Quaternion rotacionInicialMano;
    private bool rotando = false;

    private InputAction accionGatillo;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Crear binding del gatillo índice derecho (trigger)
        accionGatillo = new InputAction(
            name: "TriggerDerecho",
            type: InputActionType.Value,
            binding: "<XRController>{RightHand}/trigger"
        );
        accionGatillo.Enable();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnAgarrado);
        grabInteractable.selectExited.AddListener(OnSoltado);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnAgarrado);
        grabInteractable.selectExited.RemoveListener(OnSoltado);
        accionGatillo.Disable();
    }

    private void OnAgarrado(SelectEnterEventArgs args)
    {
        interactorTransform = args.interactorObject.transform;
    }

    private void OnSoltado(SelectExitEventArgs args)
    {
        interactorTransform = null;
        rotando = false;

        // Reactiva la rotación del jugador
        if (turnProvider != null)
            turnProvider.enabled = true;
    }

    private void Update()
    {
        if (interactorTransform == null) return;

        float valorGatillo = accionGatillo.ReadValue<float>();
        bool triggerPresionado = valorGatillo > 0.5f;

        if (triggerPresionado && !rotando)
        {
            rotando = true;
            rotacionInicialObjeto = transform.rotation;
            rotacionInicialMano = interactorTransform.rotation;

            if (turnProvider != null)
                turnProvider.enabled = false;
        }
        else if (!triggerPresionado && rotando)
        {
            rotando = false;

            if (turnProvider != null)
                turnProvider.enabled = true;
        }

        if (rotando)
        {
            Quaternion deltaRotacion = interactorTransform.rotation * Quaternion.Inverse(rotacionInicialMano);
            Quaternion objetivo = deltaRotacion * rotacionInicialObjeto;

            // Rota el objeto padre completo (con collider y física)
            transform.rotation = Quaternion.Slerp(transform.rotation, objetivo, Time.deltaTime * velocidadRotacion);
        }
    }
}
