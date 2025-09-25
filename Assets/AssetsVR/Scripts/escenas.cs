using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class escenas : MonoBehaviour
{
   public void simulador()
    {
        SceneManager.LoadScene("Juego");
    }

    public void tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }
}
