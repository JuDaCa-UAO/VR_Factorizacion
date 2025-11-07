using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Retroalimentacion : MonoBehaviour
{
    
   

    [Header("Haptics CORRECTO")]
    public float intensidadCorrecto = 0.4f;
    public float duracionCorrecto = 0.1f;

    [Header("Haptics INCORRECTO")]
    public float intensidadIncorrecto = 1.0f;
    public float duracionIncorrecto = 0.35f;

    [Header("Sockets")]
    public EspacioSocket[] sockets;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoCorrecto;    
    public AudioClip sonidoIncorrecto;  

    [Header("FX incorrecto")]
    public GameObject fxIncorrectoPrefab;  

    private XRBaseController controller;

    private void Awake()
    {
        controller = GetComponent<XRBaseController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>(); // por si el AudioSource está en el mismo objeto
    }

    private void Start()
    {
        // Si no se asignan sockets a mano, se buscan todos en la escena
        if (sockets == null || sockets.Length == 0)
        {
            sockets = FindObjectsOfType<EspacioSocket>();
        }

        // Suscribirse a los eventos de selección de cada XRSocketInteractor
        foreach (var esp in sockets)
        {
            if (esp == null) continue;

            var xrSocket = esp.GetComponent<XRSocketInteractor>();
            if (xrSocket != null)
            {
                xrSocket.selectEntered.AddListener(OnSelectEntered);
            }
        }
    }

    private void OnDestroy()
    {
        // Importante: desuscribirse al destruir este componente
        if (sockets == null) return;

        foreach (var esp in sockets)
        {
            if (esp == null) continue;
            var xrSocket = esp.GetComponent<XRSocketInteractor>();
            if (xrSocket != null)
            {
                xrSocket.selectEntered.RemoveListener(OnSelectEntered);
            }
        }
    }

    // Se llama cuando un bloque entra en cualquier socket suscrito
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var xrSocket = args.interactorObject as XRSocketInteractor;
        if (xrSocket == null) return;

        var espacio = xrSocket.GetComponent<EspacioSocket>();
        if (espacio == null) return;

        Transform bloqueTransform = args.interactableObject.transform;
        var bloque = bloqueTransform.GetComponent<BloqueAlgebraico>();

        bool esCorrecto = EsBloqueCorrecto(espacio, bloque, bloqueTransform);

        if (esCorrecto)
        {
            FeedbackCorrecto(bloqueTransform.position);
        }
        else
        {
            FeedbackIncorrecto(bloqueTransform);   // 👈 pasamos el transform
        }
    }


    // Lógica de validación (duplicamos aquí, sin tocar EspacioSocket)
    private bool EsBloqueCorrecto(EspacioSocket espacio, BloqueAlgebraico bloque, Transform tBloque)
    {
        if (espacio == null || bloque == null) return false;

        // 1) Tipo correcto
        bool tipoOk = (bloque.tipo == espacio.tipoEsperado);
        if (!tipoOk) return false;

        // 2) Orientación (si se requiere)
        if (!espacio.requiereOrientacion) return true;

        float yActual = tBloque.eulerAngles.y;
        float yEsperada = (espacio.orientacionEsperada == OrientacionEsperada.Horizontal) ? 0f : 90f;
        float diferencia = Mathf.DeltaAngle(yActual, yEsperada);

        return Mathf.Abs(diferencia) <= 10f; // tolerancia de 10°
    }

    private void FeedbackCorrecto(Vector3 posicionBloque)
    {
        // Vibración suave
        Vibrar(intensidadCorrecto, duracionCorrecto);

        // Sonido correcto
        if (audioSource != null && sonidoCorrecto != null)
        {
            audioSource.PlayOneShot(sonidoCorrecto);
        }

        
    }

    private void FeedbackIncorrecto(Transform bloqueTransform)
    {
        // Vibración más fuerte y larga
        Vibrar(intensidadIncorrecto, duracionIncorrecto);

        // Sonido incorrecto
        if (audioSource != null && sonidoIncorrecto != null)
        {
            audioSource.PlayOneShot(sonidoIncorrecto);
        }

        if (fxIncorrectoPrefab != null && bloqueTransform != null)
        {
            // posición exactamente donde está el bloque
            Vector3 pos = bloqueTransform.position;

            // si quieres un pelín más arriba:
            // pos += Vector3.up * 0.05f;

            Instantiate(fxIncorrectoPrefab, pos, bloqueTransform.rotation);
            // o Quaternion.identity si no quieres la rotación del bloque
        }
    }

    private void Vibrar(float intensidad, float duracion)
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(intensidad, duracion);
        }
        else
        {
            Debug.LogWarning("VibracionPorSocket: no se encontró XRBaseController en este objeto.");
        }
    }
}
