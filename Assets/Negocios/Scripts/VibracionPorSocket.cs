using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VibracionPorSockets : MonoBehaviour
{
    [Header("Configuración")]
    public bool esManoIzquierda = true;
    public float intensidad = 0.6f;
    public float duracion = 0.15f;

    [Header("Socket targets")]
    public EspacioSocket[] sockets;

    private bool[] estadoAnterior;
    private XRBaseController controller;

    void Awake()
    {
        // Intenta obtener cualquier controlador derivado de XRBaseController
        controller = GetComponent<XRBaseController>();
        if (controller == null)
        {
            // Algunos rigs usan ActionBasedController (deriva de XRBaseController)
            controller = GetComponent<ActionBasedController>();
        }
    }

    void Start()
    {
        if (sockets == null || sockets.Length == 0)
        {
            Debug.LogWarning("⚠️ [VibracionPorSockets] 'sockets' sin asignar o vacío.");
            estadoAnterior = System.Array.Empty<bool>();
            return;
        }

        estadoAnterior = new bool[sockets.Length];

        // Revisa elementos null de una vez
        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i] == null)
            {
                Debug.LogError($"❌ [VibracionPorSockets] sockets[{i}] es NULL. Asigna un EspacioSocket válido en el inspector.");
            }
        }
    }

    void Update()
    {
        if (sockets == null || sockets.Length == 0) return;

        // Si el tamaño cambió en tiempo de ejecución, re-sincroniza
        if (estadoAnterior == null || estadoAnterior.Length != sockets.Length)
        {
            estadoAnterior = new bool[sockets.Length];
            Debug.Log("[VibracionPorSockets] estadoAnterior re-sincronizado con sockets.Length.");
        }

        for (int i = 0; i < sockets.Length; i++)
        {
            var socket = sockets[i];

            if (socket == null)
            {
                // Evita el NullReference y avisa una sola vez por índice
                // (puedes quitar este log si ya lo corregiste en el inspector)
                Debug.LogError($"❌ [VibracionPorSockets] sockets[{i}] es NULL. Revisa el inspector.");
                continue;
            }

            // A partir de aquí, 'socket' NO es null
            bool correcto = socket.bloqueCorrecto;

            if (correcto && !estadoAnterior[i])
            {
                Vibrar();
                estadoAnterior[i] = true;
            }
            else if (!correcto && estadoAnterior[i])
            {
                estadoAnterior[i] = false;
            }
        }
    }

    void Vibrar()
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(intensidad, duracion);
            // Debug.Log($"✅ Vibración enviada a {(esManoIzquierda ? "izquierda" : "derecha")}");
        }
        else
        {
            Debug.LogWarning("❌ [VibracionPorSockets] No se encontró un XRBaseController/ActionBasedController en este objeto.");
        }
    }
}
