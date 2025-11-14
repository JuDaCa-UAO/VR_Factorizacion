using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video; // <-- para VideoPlayer

[RequireComponent(typeof(AudioSource))]
public class audiosFC : MonoBehaviour
{
    [Header("Clips principales (Audio 1, 2, 3, ...)")]
    public List<AudioClip> playlist = new List<AudioClip>();

    [Header("Iniciar automáticamente al cargar la escena")]
    public bool playOnStart = true;

    [Header("Pistas (hints) - se reproducen con el botón de pistas")]
    public List<AudioClip> hintPlaylist = new List<AudioClip>();

    [Header("Cierre")]
    public AudioClip audio4; // clip final cuando el rompecabezas está correcto (según GameManager)

    [Header("UI (opcional)")]
    public TextMeshProUGUI toggleButtonText; // Texto del botón Activar/Desactivar

    [Header("Video (embebido en Audios)")]
    [SerializeField] private RawImage videoScreen;         // contenedor visual del video (oculto al inicio)
    [SerializeField] private VideoPlayer videoPlayer;      // VideoPlayer

    [Header("Botón de pistas (opcional)")]
    [SerializeField] public Button hintButton;

    // ====== NUEVO: Validación via GameManager ======
    [Header("Validación Global (GameManager)")]
    [Tooltip("Arrastra aquí el GameObject que tiene GestorValidacionGlobal")]
    public GameObject gameManager; // <- referencia al objeto que posee el Gestor
    private GestorValidacionGlobal _gestor;  // <- cache del componente
    [Tooltip("Si está activo, solo permitirá disparar el cierre cuando la playlist principal haya terminado.")]
    public bool requerirFinDePlaylistParaCerrar = false;

    private AudioSource _as;
    private int _currentIndex = 0;

    // Estado global (botón activar/desactivar)
    private bool _globallyEnabled = true;     // si false, no suena nada
    private bool _userPausedGlobally = false; // “desactivar” pulsado por el usuario
    private float _savedTime = 0f;
    private bool _wasPlayingLastFrame = false;

    // Estado de pistas
    private bool _isHintMode = false;
    private int _currentHintIndex = 0;
    private bool _mainCompleted = false;

    [Header("FX para pistas")]
    public GameObject hintFxPrefab;          // Prefab que aparecerá al usar la pista
    public Transform hintFxSpawnPoint;       // Punto base donde se instanciará
    public Vector3 hintFxPositionOffset;     // Offset desde ese punto
    public Vector3 hintFxRotationEuler = Vector3.zero; // Rotación del FX (en grados)
    public Vector3 hintFxScale = Vector3.one;          // Escala del FX

    // internos para controlar el FX actual
    private GameObject _currentHintFxInstance;
    private Coroutine _hintFxCoroutine;

    // Cierre
    private bool _closureStarted = false; // evita disparar la secuencia dos veces

    // Expuestos (lectura)
    public bool IsGloballyEnabled => _globallyEnabled && !_userPausedGlobally;
    public bool IsHintMode => _isHintMode;
    public AudioClip CurrentClip => _as != null ? _as.clip : null;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
        _as.playOnAwake = false;
        _as.loop = false;

        // Video oculto de inicio
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.gameObject.SetActive(false);
        }
        if (videoScreen != null) videoScreen.gameObject.SetActive(false);

        // Obtener el Gestor desde el GameManager (si se asignó)
        if (gameManager != null)
        {
            _gestor = gameManager.GetComponent<GestorValidacionGlobal>();
            if (_gestor == null)
                Debug.LogWarning("[audiosFC] El GameObject asignado no tiene GestorValidacionGlobal.");
        }
        else
        {
            Debug.LogWarning("[audiosFC] No se asignó GameManager para validación global.");
        }
    }

    private void Start()
    {
        SetToggleLabel(desactivar: true);

        if (hintButton != null) hintButton.interactable = false;

        if (playlist != null && playlist.Count > 0)
        {
            _currentIndex = Mathf.Clamp(_currentIndex, 0, playlist.Count - 1);
            _as.clip = playlist[_currentIndex];

            if (playOnStart) TryPlay();
        }
    }

    private void Update()
    {
        // 1) Avance automático SOLO para la lista principal (si no estamos en pistas)
        if (IsGloballyEnabled && !_isHintMode)
        {
            if (_wasPlayingLastFrame && !_as.isPlaying)
            {
                // ¿era el ÚLTIMO clip del playlist?
                if (_currentIndex >= playlist.Count - 1)
                {
                    _mainCompleted = true;
                    if (hintButton != null) hintButton.interactable = true;
                    // se queda en silencio
                }
                else
                {
                    GoToNextClip();
                }
            }
        }

        // 2) NUEVO: si el GameManager dice que el puzzle está completo -> dispara cierre (una sola vez)
        if (!_closureStarted && _gestor != null && _gestor.bloquesCompletos)
        {
            if (!requerirFinDePlaylistParaCerrar || _mainCompleted)
            {
                _closureStarted = true;
                StartCoroutine(ClosureSequence()); // audio4 -> espera 3s -> video
            }
        }

        _wasPlayingLastFrame = _as.isPlaying;
    }

    // ===== Reproducción principal =====
    private void TryPlay()
    {
        if (!IsGloballyEnabled || _as.clip == null) return;

        if (_savedTime > 0f && _savedTime < _as.clip.length)
        {
            _as.time = _savedTime;
            _savedTime = 0f;
        }
        else
        {
            _as.time = 0f;
        }
        _as.Play();
    }

    private void GoToNextClip()
    {
        if (!IsGloballyEnabled) return;

        _currentIndex++;
        if (_currentIndex >= playlist.Count)
        {
            _currentIndex = playlist.Count - 1; // queda al final
            return;
        }

        _isHintMode = false;
        _as.clip = playlist[_currentIndex];
        _as.time = 0f;
        _as.Play();
    }

    public void PlayIndex(int index)
    {
        if (playlist == null || playlist.Count == 0) return;

        index = Mathf.Clamp(index, 0, playlist.Count - 1);
        _currentIndex = index;
        _savedTime = 0f;

        _isHintMode = false;
        _as.Stop();
        _as.clip = playlist[_currentIndex];

        if (IsGloballyEnabled)
        {
            _as.time = 0f;
            _as.Play();
            SetToggleLabel(desactivar: true);
        }
    }

    // ===== Botón de PISTAS =====
    public void PlayNextHint()
    {
        if (!_mainCompleted) return; // no permitir hasta terminar la principal
        if (hintPlaylist == null || hintPlaylist.Count == 0) return;

        _isHintMode = true;
        _currentHintIndex = Mathf.Clamp(_currentHintIndex, 0, hintPlaylist.Count - 1);

        _as.Stop();
        _as.clip = hintPlaylist[_currentHintIndex];
        _as.time = 0f;

        if (IsGloballyEnabled)
        {
            _as.Play();
            SetToggleLabel(desactivar: true);
        }

        _currentHintIndex = (_currentHintIndex + 1) % hintPlaylist.Count;

        SpawnHintFx();
    }

    private void SpawnHintFx()
    {
        if (hintFxPrefab == null) return;

        if (_currentHintFxInstance != null)
        {
            Destroy(_currentHintFxInstance);
            _currentHintFxInstance = null;
        }

        Vector3 basePos = hintFxSpawnPoint != null ? hintFxSpawnPoint.position : transform.position;
        Vector3 finalPos = basePos + hintFxPositionOffset;
        Quaternion finalRot = Quaternion.Euler(hintFxRotationEuler);

        _currentHintFxInstance = Instantiate(hintFxPrefab, finalPos, finalRot);
        _currentHintFxInstance.transform.localScale = hintFxScale;

        if (_hintFxCoroutine != null)
            StopCoroutine(_hintFxCoroutine);
        _hintFxCoroutine = StartCoroutine(DestroyHintFxWhenCurrentClipEnds());
    }

    private IEnumerator DestroyHintFxWhenCurrentClipEnds()
    {
        float waitTime = 1f;

        if (_as != null && _as.clip != null && _as.clip.length > 0f)
        {
            waitTime = _as.clip.length;
        }

        yield return new WaitForSeconds(waitTime);

        if (_currentHintFxInstance != null)
        {
            Destroy(_currentHintFxInstance);
            _currentHintFxInstance = null;
        }

        _hintFxCoroutine = null;
    }

    // ===== Botón ACTIVAR/DESACTIVAR =====
    public void ToggleActivate()
    {
        if (_as.clip == null) return;

        if (IsGloballyEnabled)
        {
            _savedTime = _as.isPlaying ? _as.time : _savedTime;
            _as.Stop();
            _userPausedGlobally = true;
            _globallyEnabled = false;
            SetToggleLabel(desactivar: false); // "Activar"
        }
        else
        {
            _globallyEnabled = true;
            _userPausedGlobally = false;

            if (_as.clip == null)
            {
                if (_isHintMode && hintPlaylist.Count > 0)
                    _as.clip = hintPlaylist[Mathf.Clamp(_currentHintIndex - 1, 0, hintPlaylist.Count - 1)];
                else if (playlist.Count > 0)
                    _as.clip = playlist[Mathf.Clamp(_currentIndex, 0, playlist.Count - 1)];
            }

            TryPlay();
            SetToggleLabel(desactivar: true); // "Desactivar"
        }
    }

    public void RestartCurrent()
    {
        if (_as.clip == null) return;

        _savedTime = 0f;
        _as.Stop();
        _as.time = 0f;

        if (IsGloballyEnabled)
        {
            _as.Play();
            SetToggleLabel(desactivar: true);
        }
    }

    // ====== CIERRE: ahora gatillado por _gestor.bloquesCompletos ======
    private IEnumerator ClosureSequence()
    {
        // Salir de pistas
        _isHintMode = false;

        // 1) Reproducir audio final (opcional)
        if (audio4 != null)
        {
            _as.Stop();
            _as.clip = audio4;
            _as.time = 0f;

            if (IsGloballyEnabled)
                _as.Play();

            if (audio4.length > 0f)
                yield return new WaitForSeconds(audio4.length);
        }

        // 2) Espera 3s y muestra video
        yield return new WaitForSeconds(3f);
        ShowVideoWithAnimation();
    }

    // ====== VIDEO (dentro de Audios) ======
    private void ShowVideoWithAnimation()
    {
        if (videoScreen != null) videoScreen.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play(); // reproduce imagen + audio del video
        }
    }

    private void SetToggleLabel(bool desactivar)
    {
        if (toggleButtonText == null) return;
        toggleButtonText.text = desactivar ? "Desactivar" : "Activar";
    }
}
