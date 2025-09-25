using System.Collections;
using UnityEngine;

public class Chorro : MonoBehaviour
{
    public Transform firepoint;
    public LineRenderer lineRenderer;
    public GameObject shootFxPrefab;

    private ParticleSystem shootFx;
    private bool isChorreando = false;
    public AudioSource audioSource;

    public string tipo;  // Tipo de extintor que está usando este objeto

    public LayerMask Fuego; // LayerMask para detectar objetos en la capa "Fuego"
    public GameObject canvaError;

    // Método para iniciar el chorro
    public void StartChorrear()
    {
        if (!isChorreando)
        {
            isChorreando = true;

            if (shootFx != null)
            {
                Destroy(shootFx.gameObject);
            }

            shootFx = Instantiate(shootFxPrefab, firepoint.position, firepoint.rotation).GetComponent<ParticleSystem>();
            shootFx.Play();
            audioSource.Play();
            StartCoroutine(Chorreando());
        }
    }

    // Método para detener el chorro
    public void StopChorrear()
    {
        if (isChorreando)
        {
            isChorreando = false;
            lineRenderer.enabled = false;

            if (shootFx != null)
            {
                shootFx.Stop();
                audioSource.Stop();
                Destroy(shootFx.gameObject, 0.5f);
            }
        }
    }

    IEnumerator Chorreando()
    {
        while (isChorreando)
        {
            RaycastHit hit;
            bool hitInfo = Physics.Raycast(firepoint.position, firepoint.forward, out hit, Mathf.Infinity, Fuego);

            if (hitInfo)
            {
                lineRenderer.SetPosition(0, firepoint.position);
                lineRenderer.SetPosition(1, hit.point);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Fuego"))
                {
                    Fire fireComponent = hit.collider.GetComponent<Fire>();

                    if (fireComponent != null)
                    {
                        switch (tipo)
                        {
                            case "Agua":
                                if (hit.collider.gameObject.tag == "TipoA")
                                {
                                    fireComponent.Extinguish();  // Apaga el fuego y registra en GameManager
                                }
                                else
                                {
                                    StartCoroutine(ShowError());
                                }
                                break;

                            case "Espuma":
                                if (hit.collider.gameObject.tag == "TipoB")
                                {
                                    fireComponent.Extinguish();
                                }
                                else
                                {
                                    StartCoroutine(ShowError());
                                }
                                break;

                            case "Polvo":
                                if (hit.collider.gameObject.tag == "TipoC")
                                {
                                    fireComponent.Extinguish();
                                }
                                else
                                {
                                    StartCoroutine(ShowError());
                                }
                                break;

                            case "CO2":
                                if (hit.collider.gameObject.tag == "TipoD")
                                {
                                    fireComponent.Extinguish();
                                }
                                else
                                {
                                    StartCoroutine(ShowError());
                                }
                                break;

                            default:
                                StartCoroutine(ShowError());
                                break;
                        }
                    }
                }
            }
            else
            {
                lineRenderer.SetPosition(0, firepoint.position);
                lineRenderer.SetPosition(1, firepoint.position + firepoint.forward * 20);
            }

            if (shootFx != null)
            {
                shootFx.transform.position = firepoint.position;
                shootFx.transform.rotation = firepoint.rotation;
            }

            lineRenderer.enabled = true;
            yield return new WaitForSeconds(0.02f);
        }

        lineRenderer.enabled = false;
    }

    IEnumerator ShowError()
    {
        canvaError.SetActive(true);
        yield return new WaitForSeconds(2f);
        canvaError.SetActive(false);
    }
}