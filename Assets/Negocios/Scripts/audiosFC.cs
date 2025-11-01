using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // para el label del botón

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
    public AudioClip audio4; // clip final cuando todos los bloques están correctos

    [Header("UI (opcional)")]
    public TextMeshProUGUI toggleButtonText; // Texto del botón Activar/Desactivar

    // ====== VIDEO embebido en este script ======
    [Header("Video (embebido en Audios)")]
    [SerializeField] private UnityEngine.UI.RawImage videoScreen; // contenedor visual del video (oculto al inicio)
    [SerializeField] private UnityEngine.Video.VideoPlayer videoPlayer; // VideoPlayer
   

    private AudioSource _as;
    private int _currentIndex = 0;

    // Estado global (botón activar/desactivar)
    private bool _globallyEnabled = true;     // si false, no suena nada
    private bool _userPausedGlobally = false; // “desactivar” pulsado por el usuario
    private float _savedTime = 0f;
    private bool _wasPlayingLastFrame = false;

    // Estado de pistas
    [SerializeField] public Button hintButton;
    private bool _isHintMode = false;
    private int _currentHintIndex = 0;
    private bool _mainCompleted = false;

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

        // Configuración segura del Video al inicio
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;        
            videoPlayer.gameObject.SetActive(false); // que NO se oiga ni vea al inicio
        }
        if (videoScreen != null) videoScreen.gameObject.SetActive(false);
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
        // Avance automático SOLO para la lista principal
        if (IsGloballyEnabled && !_isHintMode)
        {
            if (_wasPlayingLastFrame && !_as.isPlaying)
            {
                // ¿era el ÚLTIMO clip del playlist?
                if (_currentIndex >= playlist.Count - 1)
                {
                    // Marcar lista principal como completada y habilitar botón de pistas
                    _mainCompleted = true;                        // <-- nuevo
                    if (hintButton != null) hintButton.interactable = true; // <-- nuevo
                    // Nos quedamos en silencio (no llamamos GoToNextClip)
                }
                else
                {
                    GoToNextClip();
                }
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
        // Si el botón está deshabilitado, no hará falta esta guardia, pero por seguridad:
        if (!_mainCompleted) return; // <-- opcional, evita disparar pistas si no terminó la lista principal

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
    }

    // ===== Botón ACTIVAR/DESACTIVAR =====
    public void ToggleActivate()
    {
        if (_as.clip == null) return;

        if (IsGloballyEnabled)
        {
            // DESACTIVAR
            _savedTime = _as.isPlaying ? _as.time : _savedTime;
            _as.Stop();
            _userPausedGlobally = true;
            _globallyEnabled = false;
            SetToggleLabel(desactivar: false); // "Activar"
        }
        else
        {
            // ACTIVAR
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

    // ====== CIERRE: llamado cuando TODOS los bloques están correctos ======
    public void CheckAllBlocksSnapped()
    {
        if (_closureStarted) return;          // no repetir
        if (!AllBlocksSnapped()) return;

        _closureStarted = true;
        StartCoroutine(ClosureSequence());    // audio4 -> esperar 3s -> video (con animación)
    }

    private IEnumerator ClosureSequence()
    {
        // salir de pistas y forzar audio4
        _isHintMode = false;

        if (audio4 != null)
        {
            _as.Stop();
            _as.clip = audio4;
            _as.time = 0f;

            if (IsGloballyEnabled)
                _as.Play();

            // esperar a que termine audio4
            if (audio4.length > 0f)
                yield return new WaitForSeconds(audio4.length);
        }

        // +3 segundos
        yield return new WaitForSeconds(3f);

        // Mostrar video (desde aquí controlamos todo lo visual/sonoro del video)
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

    // ====== Utilidades ======
    private bool AllBlocksSnapped()
    {
        var blocks = Object.FindObjectsByType<BlockSnapState>(FindObjectsSortMode.None);
        foreach (var b in blocks) if (!b.isSnapped) return false;
        return true;
    }

    private void SetToggleLabel(bool desactivar)
    {
        if (toggleButtonText == null) return;
        toggleButtonText.text = desactivar ? "Desactivar" : "Activar";
    }
}
