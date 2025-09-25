using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuegoToggle : MonoBehaviour
{
    public Toggle Check;
    
    public void Chechear()
    {
        Check.isOn = true;
    }
    
    void OnDestroy()
    { Chechear(); }
}
