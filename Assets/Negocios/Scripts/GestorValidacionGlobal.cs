using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GestorValidacionGlobal : MonoBehaviour
{
    [Header("Sockets a validar")]
    public EspacioSocket[] todosLosSockets;

    [Header("Referencia al sistema de puntuación")]
    public ScoreButton scoreButton; // 👈 arrastra aquí el objeto con el script ScoreButton

    private bool puntosYaSumados = false; // 👈 evita que sume varias veces

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
            if (!puntosYaSumados)
            {
                // ✅ Agrega un punto usando el sistema de ScoreButton
                if (scoreButton != null)
                {
                    scoreButton.AddPoint();
                    Debug.Log("🏆 Todos los bloques están correctos. ¡Punto agregado!");
                }
                else
                {
                    Debug.LogWarning("⚠️ No se asignó el ScoreButton al Gestor de Validación.");
                }

                puntosYaSumados = true;
            }

            Debug.Log("🎉 Todos los bloques están bien ubicados ✅");
        }
        else
        {
            // Si algo cambia (alguien quita un bloque), permite sumar nuevamente la próxima vez
            if (puntosYaSumados)
            {
                Debug.Log("🔁 Un bloque fue removido o está mal colocado. Esperando nueva validación completa.");
            }

            puntosYaSumados = false;
        }
    }
}
