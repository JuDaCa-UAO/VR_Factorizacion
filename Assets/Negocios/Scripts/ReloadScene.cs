using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar las escenas

public class ReloadScene : MonoBehaviour
{
    // Método que recarga la escena actual
    public void ReloadCurrentScene()
    {
        // Obtener el nombre de la escena actual
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Cargar la misma escena de nuevo
        SceneManager.LoadScene(currentSceneName);
    }
}
