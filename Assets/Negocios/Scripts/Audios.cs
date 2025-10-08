using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Audios : MonoBehaviour
{
    [Header("Clips en orden (Audio 1, 2, 3, ...)")]
    public List<AudioClip> playlist = new List<AudioClip>();

    [Header("Iniciar automáticamente al cargar la escena")]
    public bool playOnStart = true;

    [Header("Nuevo Audio (cuando todos los bloques estén en su lugar)")]
    public AudioClip audio4; // Audio especial final
    public Dialogue dialogueController;

    [Header("UI (opcional)")]
    public TextMeshProUGUI toggleButtonText;

    private AudioSource _as;
    private int _currentIndex = 0;

    private bool _globallyEnabled = true;
    private bool _userPausedGlobally = false;

    private float _savedTime = 0f;
    private bool _wasPlayingLastFrame = false;

    [Header("Animación (RawImage + VideoPlayer)")]
    [SerializeField] private RawImage animationRawImage;   // ← Asigna aquí el RawImage
    [SerializeField] private VideoPlayer videoPlayer;       // ← Asigna aquí el VideoPlayer

    private bool playingFinalAudio = false; // para controlar el audio 4

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
        _as.playOnAwake = false;
        _as.loop = false;
    }

    private void Start()
    {
        SetToggleLabel(desactivar: true);

        if (playlist == null || playlist.Count == 0) return;

        _currentIndex = Mathf.Clamp(_currentIndex, 0, playlist.Count - 1);
        _as.clip = playlist[_currentIndex];

        if (playOnStart)
            TryPlay();

        // 🔹 Aseguramos que ambos estén desactivados al inicio
        if (animationRawImage != null)
            animationRawImage.gameObject.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(false);
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    private void Update()
    {
        if (_globallyEnabled && !_userPausedGlobally)
        {
            if (_wasPlayingLastFrame && !_as.isPlaying)
            {
                if (playingFinalAudio)
                {
                    playingFinalAudio = false;
                    Invoke(nameof(ShowAnimationCanvas), 3f);
                }
                else
                {
                    GoToNextClip();
                }
            }
        }

        _wasPlayingLastFrame = _as.isPlaying;
    }

    private void TryPlay()
    {
        if (!_globallyEnabled || _as.clip == null) return;

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
        if (!_globallyEnabled || _userPausedGlobally) return;

        _currentIndex++;
        if (_currentIndex >= playlist.Count)
        {
            _currentIndex = playlist.Count - 1;
            return;
        }

        _as.clip = playlist[_currentIndex];
        _as.time = 0f;
        _as.Play();
    }

    // 🔹 Llamado cuando todos los bloques están bien colocados
    public void CheckAllBlocksSnapped()
    {
        if (AllBlocksSnapped())
        {
            Debug.Log("✅ Todos los bloques en su lugar.");

            _as.Stop();
            playlist.Clear();

            if (audio4 != null)
            {
                _as.clip = audio4;
                _as.time = 0f;
                _as.Play();
                playingFinalAudio = true;
                Debug.Log("🎧 Reproduciendo audio final (audio4)...");
            }
            else
            {
                Debug.LogWarning("No se asignó Audio4 en el inspector.");
                Invoke(nameof(ShowAnimationCanvas), 3f);
            }
        }
    }

    private void ShowAnimationCanvas()
    {
        if (animationRawImage != null)
        {
            animationRawImage.gameObject.SetActive(true);
            Debug.Log("🎬 RawImage activado, reproduciendo video...");
        }

        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
            Debug.Log("🎥 VideoPlayer activado y reproduciendo animación...");
        }
    }

    // 🔹 Se llama automáticamente al terminar el video
    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("🎞️ Video finalizado, desactivando RawImage y VideoPlayer.");

        if (animationRawImage != null)
            animationRawImage.gameObject.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.gameObject.SetActive(false);
        }
    }

    bool AllBlocksSnapped()
    {
        var blocks = Object.FindObjectsByType<BlockSnapState>(FindObjectsSortMode.None);
        foreach (var block in blocks)
        {
            if (!block.isSnapped) return false;
        }
        return true;
    }

    // -----------------------
    // Métodos auxiliares
    // -----------------------

    public void ToggleActivate()
    {
        if (_as.clip == null) return;

        if (_globallyEnabled && !_userPausedGlobally)
        {
            _savedTime = _as.isPlaying ? _as.time : _savedTime;
            _as.Stop();

            _userPausedGlobally = true;
            _globallyEnabled = false;

            SetToggleLabel(desactivar: false);
        }
        else
        {
            _globallyEnabled = true;
            _userPausedGlobally = false;
            TryPlay();
            SetToggleLabel(desactivar: true);
        }
    }

    public void RestartCurrent()
    {
        if (_as.clip == null) return;

        _savedTime = 0f;
        _as.Stop();
        _as.time = 0f;

        if (_globallyEnabled && !_userPausedGlobally)
        {
            _as.Play();
            SetToggleLabel(desactivar: true);
        }
    }

    public void PlayIndex(int index)
    {
        if (playlist.Count == 0) return;

        index = Mathf.Clamp(index, 0, playlist.Count - 1);
        _currentIndex = index;
        _savedTime = 0f;

        _as.Stop();
        _as.clip = playlist[_currentIndex];

        if (_globallyEnabled && !_userPausedGlobally)
        {
            _as.time = 0f;
            _as.Play();
            SetToggleLabel(desactivar: true);
        }
    }

    private void SetToggleLabel(bool desactivar)
    {
        if (toggleButtonText == null) return;
        toggleButtonText.text = desactivar ? "Desactivar" : "Activar";
    }
}
