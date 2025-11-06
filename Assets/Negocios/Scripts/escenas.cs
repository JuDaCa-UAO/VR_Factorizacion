using UnityEngine;
using UnityEngine.SceneManagement;

public class Escenas : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    [SerializeField] private string sceneToLoad; // puedes escribir el nombre en el Inspector
    [SerializeField] private string end;
    /// <summary>
    /// Cargar la escena que está en sceneToLoad
    /// </summary>
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No se ha asignado un nombre de escena en el inspector.");
        }
    }

    public void endd()
    {
        if (!string.IsNullOrEmpty(end))
        {
            SceneManager.LoadScene(end);
        }
        else
        {
            Debug.LogWarning("No se ha asignado un nombre de escena en el inspector.");
        }
    }

    /// <summary>
    /// Método alternativo para cargar cualquier escena pasándole el nombre desde un botón o evento.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
