using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Referencias a los Canvas")]
    public GameObject canvasToActivate;
    public GameObject canvasToDeactivate;

    /// <summary>
    /// Activa un canvas y desactiva el otro.
    /// Este método debe ser asignado al evento OnClick del botón en el Inspector.
    /// </summary>
    public void SwitchCanvas()
    {
        if (canvasToActivate != null)
            canvasToActivate.SetActive(true);

        if (canvasToDeactivate != null)
            canvasToDeactivate.SetActive(false);
    }
}
