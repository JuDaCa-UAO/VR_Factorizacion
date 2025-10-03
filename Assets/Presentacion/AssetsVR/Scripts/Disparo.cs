using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disparo : MonoBehaviour
{
    public Transform firepoint;
    public int damage = 25;

    public LineRenderer lineRenderer;

    public GameObject shootFx, impact;

    public void Disparar()
    {
        StartCoroutine(Disparando());
    }

    IEnumerator Disparando()
    {
        RaycastHit hit;
        bool hitInfo = Physics.Raycast(firepoint.position, firepoint.forward, out hit);

        if (hitInfo)
        {
            lineRenderer.SetPosition(0, firepoint.position);
            lineRenderer.SetPosition(1, hit.point);

            Instantiate(impact, hit.point, Quaternion.identity);

            Instantiate(shootFx, firepoint.transform.position, firepoint.transform.rotation);
        }
        else
        {
            lineRenderer.SetPosition(0, firepoint.position);
            lineRenderer.SetPosition(1, firepoint.position + firepoint.forward * 20);
        }

        lineRenderer.enabled = true;

        yield return new WaitForSeconds(0.02f);

        lineRenderer.enabled = false;
    }
}
