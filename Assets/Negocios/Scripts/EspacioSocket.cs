using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum OrientacionEsperada
{
    Horizontal, // Y = 0
    Vertical    // Y = 90
}

public class EspacioSocket : MonoBehaviour
{
    [Header("Configuración esperada")]
    public TipoBloque tipoEsperado;
    public bool requiereOrientacion = false;
    public OrientacionEsperada orientacionEsperada = OrientacionEsperada.Horizontal;

    [Header("Visual Feedback (opcional)")]
    public Renderer feedbackRenderer;
    public Color colorCorrecto = Color.green;
    public Color colorIncorrecto = Color.red;
    public Color colorVacio = Color.white;

    [Header("Debug")]
    public bool bloqueCorrecto = false;

    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    void OnEnable()
    {
        socket.selectEntered.AddListener(OnBloqueInsertado);
        socket.selectExited.AddListener(OnBloqueRemovido);
    }

    void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnBloqueInsertado);
        socket.selectExited.RemoveListener(OnBloqueRemovido);
    }

    void OnBloqueInsertado(SelectEnterEventArgs args)
    {
        var bloque = args.interactableObject.transform.GetComponent<BloqueAlgebraico>();
        if (bloque == null)
        {
            bloqueCorrecto = false;
            ActualizarColor();
            return;
        }

        bool tipoCorrecto = bloque.tipo == tipoEsperado;
        bool orientacionCorrecta = true;

        if (requiereOrientacion)
        {
            float yActual = args.interactableObject.transform.eulerAngles.y;
            float yEsperada = (orientacionEsperada == OrientacionEsperada.Horizontal) ? 0f : 90f;
            float diferencia = Mathf.DeltaAngle(yActual, yEsperada);
            orientacionCorrecta = Mathf.Abs(diferencia) <= 10f;
        }

        bloqueCorrecto = tipoCorrecto && orientacionCorrecta;

        Debug.Log(bloqueCorrecto
            ? $"✅ {bloque.tipo} correctamente colocado en {name}"
            : $"❌ {bloque.tipo} mal orientado o tipo incorrecto en {name}");

        ActualizarColor();

        // 👇 Llama al gestor de validación global
        FindObjectOfType<GestorValidacionGlobal>()?.Validar();
    }

    void OnBloqueRemovido(SelectExitEventArgs args)
    {
        bloqueCorrecto = false;
        ActualizarColor();

        // 👇 Vuelve a validar si se retira el bloque
        FindObjectOfType<GestorValidacionGlobal>()?.Validar();
    }

    void ActualizarColor()
    {
        if (feedbackRenderer == null) return;
        feedbackRenderer.material.color = bloqueCorrecto ? colorCorrecto : colorVacio;
    }
}
