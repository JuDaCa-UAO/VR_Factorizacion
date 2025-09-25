using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextoTutorial : MonoBehaviour
{

    public TextMeshProUGUI TextMeshPro;
    public string texto;
    public string textoOriginal;
    public string[] descripciones;
    public int index = 0;

    void Start()
    {
        textoOriginal = TextMeshPro.text;
    }

    public void CambiarTexto()
    {
        TextMeshPro.text = texto;
    }

    public void DevolverTexto()
    {
        TextMeshPro.text = textoOriginal; //se usa para pasar al siguiente extintor
    }

    public void SiguienteTexto()
    {
        if (index <= 3)
        {
            TextMeshPro.text = descripciones[index];
            index++;
        }
    }

}
