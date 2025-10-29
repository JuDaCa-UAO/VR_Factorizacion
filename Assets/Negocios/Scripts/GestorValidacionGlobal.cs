using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GestorValidacionGlobal : MonoBehaviour
{
    public EspacioSocket[] todosLosSockets;

    public void Validar()
    {
        if (todosLosSockets == null || todosLosSockets.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay sockets asignados.");
            return;
        }

        bool todoCorrecto = true;

        foreach (var socket in todosLosSockets)
        {
            XRSocketInteractor interactor = socket.GetComponent<XRSocketInteractor>();
            if (interactor == null || !interactor.hasSelection || !socket.bloqueCorrecto)
            {
                todoCorrecto = false;
                break;
            }
        }

        if (todoCorrecto)
        {
            Debug.Log("🎉 Todos los bloques están bien ubicados ✅");
            // Aquí podrías activar UI, sonidos, animaciones, etc.
        }
    }
}
