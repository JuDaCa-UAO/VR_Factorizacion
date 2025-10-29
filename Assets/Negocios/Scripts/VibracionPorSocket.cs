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

    void Start()
    {
        controller = GetComponent<XRBaseController>();
        estadoAnterior = new bool[sockets.Length];
    }

    void Update()
    {
        for (int i = 0; i < sockets.Length; i++)
        {
            var socket = sockets[i];

            if (socket.bloqueCorrecto && !estadoAnterior[i])
            {
                Vibrar();
                estadoAnterior[i] = true;
            }
            else if (!socket.bloqueCorrecto)
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
            Debug.Log($"✅ Vibración enviada a {(esManoIzquierda ? "izquierda" : "derecha")}");
        }
        else
        {
            Debug.LogWarning("❌ No se encontró XRBaseController.");
        }
    }
}
