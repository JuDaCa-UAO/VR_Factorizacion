using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class Audios : MonoBehaviour
{
    [Header("Clips en orden (Audio 1, 2, 3, ...)")]
    public List<AudioClip> playlist = new List<AudioClip>();

    [Header("Iniciar automáticamente al cargar la escena")]
    public bool playOnStart = true;

    [Header("UI (opcional)")]
    public TextMeshProUGUI toggleButtonText;

    private AudioSource _as;
    private int _currentIndex = 0;

    private bool _globallyEnabled = true;
    private bool _userPausedGlobally = false;
    private float _savedTime = 0f;
    private bool _wasPlayingLastFrame = false;

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
    }

    private void Update()
    {
        if (_globallyEnabled && !_userPausedGlobally)
        {
            if (_wasPlayingLastFrame && !_as.isPlaying)
            {
                GoToNextClip();
            }
        }

        _wasPlayingLastFrame = _as.isPlaying;
    }

    // 🎧 Reproducir desde donde quedó o desde el inicio
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

    // 🎧 Pasa al siguiente clip de la lista
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

    // 🔘 Botón para pausar o reanudar
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

    // 🔁 Reinicia el clip actual
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

    // ▶️ Reproduce un clip específico
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

    // Cambia el texto del botón (si lo hay)
    private void SetToggleLabel(bool desactivar)
    {
        if (toggleButtonText == null) return;
        toggleButtonText.text = desactivar ? "Desactivar" : "Activar";
    }
}
