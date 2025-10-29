using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;

public class PrefabDispenser : MonoBehaviour
{
    [Header("Prefab a dispensar")]
    public GameObject prefab;

    [Header("Punto de aparición")]
    public Transform spawnPoint;

    [Header("Opciones")]
    public float respawnDelay = 0f;
    public bool parentSpawnedToDispenser = true;

    private GameObject current;
    private readonly List<GameObject> spawnedPrefabs = new List<GameObject>();

    void Awake()
    {
        if (spawnPoint == null) spawnPoint = transform;
    }

    void OnEnable()
    {
        EnsureOne();
    }

    void OnDisable()
    {
        CleanupAll();
    }

    void OnDestroy()
    {
        CleanupAll();
    }

    void EnsureOne()
    {
        if (current != null) return;

        current = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        spawnedPrefabs.Add(current);

        if (parentSpawnedToDispenser)
            current.transform.SetParent(transform);

        Hook(current);
    }

    void Hook(GameObject go)
    {
        if (go == null) return;
        var grab = go.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.selectEntered.AddListener(OnGrabbed);

        var notifier = go.GetComponent<OnDestroyNotifier>();
        if (notifier == null)
            notifier = go.AddComponent<OnDestroyNotifier>();

        notifier.Destroyed += OnItemDestroyed;
    }

    void Unhook(GameObject go)
    {
        if (go == null) return;
        var grab = go.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.selectEntered.RemoveListener(OnGrabbed);

        var notifier = go.GetComponent<OnDestroyNotifier>();
        if (notifier != null)
            notifier.Destroyed -= OnItemDestroyed;
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        current = null;
        EnsureOne();
    }

    void OnItemDestroyed()
    {
        if (current != null) Unhook(current);
        current = null;
        EnsureOne();
    }

    // 🔥 Limpieza global cuando termina la escena
    void CleanupAll()
    {
        foreach (var obj in spawnedPrefabs)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedPrefabs.Clear();
    }
}

public class OnDestroyNotifier : MonoBehaviour
{
    public System.Action Destroyed;
    void OnDestroy() => Destroyed?.Invoke();
}
