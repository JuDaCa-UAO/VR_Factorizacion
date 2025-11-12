using UnityEngine;

public class FinalTutorialActivator : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Objeto que contiene el GestorValidacionGlobal (p.ej. GameManager).")]
    public GameObject objetoConGestor;        // arrastra GameManager aquí

    [Tooltip("Componente que reproduce la narración inicial (Audios.cs).")]
    public MonoBehaviour audioScript;         // arrastra el componente Audios

    [Tooltip("Componente que manejará audio final + video (AudioFinalTutorial.cs).")]
    public AudioFinalTutorial audioFinalScript; // arrastra el componente AudioFinalTutorial

    [Header("Protecciones")]
    [Tooltip("Requiere que el estado 'bloquesCompletos' se mantenga X frames seguidos (antirrebote).")]
    public int framesConfirmacion = 2;

    private GestorValidacionGlobal gestor;
    private bool activado = false;
    private int framesOk = 0;

    private void Start()
    {
        if (objetoConGestor != null)
            gestor = objetoConGestor.GetComponent<GestorValidacionGlobal>();

        if (gestor == null)
            Debug.LogError("❌ [FinalActivator] No se encontró GestorValidacionGlobal en el objeto asignado.");

        // Por si en el inspector quedó habilitado, garantizamos que NO arranque solo.
        if (audioFinalScript != null)
            audioFinalScript.ResetState(); // apaga RawImage/Video y deja listo sin iniciar
    }

    private void Update()
    {
        if (activado || gestor == null) return;

        if (gestor.bloquesCompletos)
        {
            framesOk++;
            if (framesOk >= framesConfirmacion)
            {
                ActivarFinal();
                activado = true;
            }
        }
        else
        {
            framesOk = 0;
        }
    }

    private void ActivarFinal()
    {
        Debug.Log("🎯 [FinalActivator] Bloques completos confirmados. Lanzando final...");

        if (audioScript != null) audioScript.enabled = false;

        if (audioFinalScript != null)
            audioFinalScript.IniciarSecuencia();
        else
            Debug.LogError("❌ [FinalActivator] Falta referencia a AudioFinalTutorial.");
    }
}
