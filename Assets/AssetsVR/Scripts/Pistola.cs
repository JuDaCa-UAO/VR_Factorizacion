using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pistola : MonoBehaviour
{
    public XRGrabInteractable grabInteractable;

    public Disparo disparo;

    public GameObject shootFX;

    void Start()
    {
        grabInteractable.activated.AddListener(x => Disparando());
        grabInteractable.deactivated.AddListener(x => DejarDisparo());
    }

    public void Disparando()
    {
        disparo.Disparar();
    }

    public void DejarDisparo()
    {
        //Espacio para los efectos al dejar de disparar
        Debug.Log("Disparandont");
    }
}
