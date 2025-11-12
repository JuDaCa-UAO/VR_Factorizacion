using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GestorValidacionGlobal : MonoBehaviour
{
    [Header("Sockets a validar")]
    public EspacioSocket[] todosLosSockets;

    [Header("Referencia al temporizador")]
    public LevelTimer levelTimer;

    [Header("Referencia al sistema de puntuación")]
    public ScoreButton scoreButton;

    [Header("Estado general")]
    [Tooltip("Se pone en TRUE cuando TODOS los bloques correctos están encajados.")]
    public bool bloquesCompletos = false;

    private bool puntosYaSumados = false;

    // Llama a este método cuando cambie el estado de algún socket/bloque
    public void Validar()
    {
        if (todosLosSockets == null || todosLosSockets.Length == 0)
        {
            Debug.LogWarning("⚠️ [Gestor] No hay sockets asignados.");
            bloquesCompletos = false;
            return;
        }

        bool todoCorrecto = true;

        foreach (var socket in todosLosSockets)
        {
            var interactor = socket.GetComponent<XRSocketInteractor>();
            if (interactor == null || !interactor.hasSelection || !socket.bloqueCorrecto)
            {
                todoCorrecto = false;
                break;
            }
        }

        bloquesCompletos = todoCorrecto;

        if (todoCorrecto)
        {
            if (!puntosYaSumados)
            {
                if (scoreButton != null) scoreButton.AddPoint();
                if (levelTimer != null) levelTimer.PauseTimer();
                puntosYaSumados = true;
            }
            // Debug opcional:
            // Debug.Log("🎉 [Gestor] Todos los bloques correctos.");
        }
        else
        {
            if (puntosYaSumados)
            {
                // Debug opcional:
                // Debug.Log("🔁 [Gestor] Algún bloque se removió o está incorrecto.");
            }
            puntosYaSumados = false;
        }
    }
}
