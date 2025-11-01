using UnityEngine;

public enum TipoBloque { XCuadrado, X, Unidad }

public class BloqueAlgebraico : MonoBehaviour
{
    [Header("Configuración del bloque")]
    public TipoBloque tipo;

    [Tooltip("Rotación base del bloque (para normalizar la orientación de referencia)")]
    public Vector3 orientacionBase = Vector3.zero;
}
