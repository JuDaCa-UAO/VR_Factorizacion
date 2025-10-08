using UnityEngine;

public class Facto : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefab1; // Prefab 1 que se instanciará al inicio
    public GameObject prefab2; // Prefab 2 que se instanciará después

    private GameObject instantiatedPrefab1; // Variable para guardar la instancia de prefab 1
    private GameObject instantiatedPrefab2; // Variable para guardar la instancia de prefab 2

    public BlockDragger blockDragger; // Referencia a BlockDragger para verificar el estado de los bloques

    // Valores de posición, rotación y escala proporcionados
    private Vector3 spawnPosition = new Vector3(-41.8f, -1.8f, -6.9f);
    private Quaternion spawnRotation = Quaternion.Euler(28.814f, -89.14f, -88.23f);
    private Vector3 spawnScale = new Vector3(585.06f, 585.06f, 585.06f);

    void Start()
    {
        // Instanciamos prefab 1 al inicio con los valores de posición, rotación y escala
        instantiatedPrefab1 = Instantiate(prefab1, spawnPosition, spawnRotation);
        instantiatedPrefab1.transform.localScale = spawnScale;  // Aplicamos la escala
    }

    void Update()
    {
        // Verificamos si todos los bloques están correctamente posicionados
        if (blockDragger.AllBlocksSnapped() && instantiatedPrefab2 == null)
        {
            // Si todos los bloques están en su lugar y aún no se ha instanciado el prefab 2
            SpawnPrefab2();
        }
    }

    void SpawnPrefab2()
    {
        // Instanciamos prefab 2 en la misma posición, rotación y escala que el prefab 1
        instantiatedPrefab2 = Instantiate(prefab2, instantiatedPrefab1.transform.position, instantiatedPrefab1.transform.rotation);
        instantiatedPrefab2.transform.localScale = instantiatedPrefab1.transform.localScale;

        // Destruimos el prefab 1
        Destroy(instantiatedPrefab1);
    }
}
