using System.Collections.Generic;
using UnityEngine;
using TMPro; // <- TextMeshPro
using Unity.VisualScripting;

[RequireComponent(typeof(AudioSource))]
public class Audios : MonoBehaviour
{
    [Header("Clips en orden (Audio 1, 2, 3, ...)")]
    public List<AudioClip> playlist = new List<AudioClip>();

    [Header("Iniciar automáticamente al cargar la escena")]
    public bool playOnStart = true;

    [Header("Nuevo Audio y Texto (cuando todos los bloques estén en su lugar)")]
    public AudioClip audio4;

    [Header("UI (opcional)")]
    public TextMeshProUGUI toggleButtonText; // Texto del botón Activar/Desactivar

    private AudioSource _as;
    private int _currentIndex = 0;

    // Estado global simple para este controlador
    private bool _globallyEnabled = true;     // cuando false, no debe sonar
    private bool _userPausedGlobally = false; // true si el usuario pulsó "desactivar"

    private float _savedTime = 0f;            // para reanudar desde donde quedó
    private bool _wasPlayingLastFrame = false;

    public BlockDragger block;

    private void Awake()
    {
        _as = GetComponent<AudioSource>();
        _as.playOnAwake = false;
        _as.loop = false;


    }

    private void Start()
    {
        // Texto inicial del botón: "desactivar"
        SetToggleLabel(desactivar: true);

        if (playlist == null || playlist.Count == 0) return;

        _currentIndex = Mathf.Clamp(_currentIndex, 0, playlist.Count - 1);
        _as.clip = playlist[_currentIndex];

        if (playOnStart)
            TryPlay();
    }

    private void Update()
    {
        // Avanza automáticamente al siguiente clip cuando termine
        if (_globallyEnabled && !_userPausedGlobally)
        {
            if (_wasPlayingLastFrame && !_as.isPlaying)
            {
                GoToNextClip();
            }
        }

        _wasPlayingLastFrame = _as.isPlaying;
    }


    // Reproducción / Secuencia

    private void TryPlay()
    {
        if (!_globallyEnabled || playlist.Count == 0 || _as.clip == null) return;

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
            // Llegó al final: quédate en silencio (o cambia a 0 si quieres loop)
            _currentIndex = playlist.Count - 1;
            return;
        }

        _as.clip = playlist[_currentIndex];
        _as.time = 0f;
        _as.Play();
    }

    public void PlayNewAudio(int index)
    {
        if (index >= 0 && index < playlist.Count)
        {
            _as.clip = playlist[index];
            _as.Play();
        }
        else
        {
            // Si el audio no está en la lista, aseguramos que exista un fallback (audio 4)
            if (audio4 != null)
            {
                _as.clip = audio4;
                _as.Play();
            }
        }
    }

    public void CheckAllBlocksSnapped()
    {
        if (AllBlocksSnapped()) // Si todos los bloques están bien colocados
        {
            // Reproducir el nuevo audio (audio 4) si existe
            PlayNewAudio(4);  // Reproduce el audio 4 desde la playlist

        }
    }

    // Método para verificar si todos los bloques están en la posición correcta
    bool AllBlocksSnapped()
    {
        var blocks = Object.FindObjectsByType<BlockSnapState>(FindObjectsSortMode.None);
        foreach (var block in blocks)
        {
            if (!block.isSnapped) return false;  // Si algún bloque no está en su lugar
        }
        return true;  // Si todos los bloques están bien, retornar true
    }

    // Métodos para botones

    public void ToggleActivate()
    {
        if (playlist.Count == 0) return;

        if (_globallyEnabled && !_userPausedGlobally)
        {
            // DESACTIVAR
            _savedTime = _as.isPlaying ? _as.time : _savedTime;
            _as.Stop();

            _userPausedGlobally = true;
            _globallyEnabled = false;

            SetToggleLabel(desactivar: false); // texto: "activar"
        }
        else
        {
            // ACTIVAR
            _globallyEnabled = true;
            _userPausedGlobally = false;

            if (_as.clip == null)
                _as.clip = playlist[_currentIndex];

            TryPlay();

            SetToggleLabel(desactivar: true); // texto: "desactivar"
        }
    }
    
    public void RestartCurrent()
    {
      
        if (playlist.Count == 0) return;

        _savedTime = 0f;
        if (_as.clip == null)
            _as.clip = playlist[_currentIndex];

        _as.Stop();
        _as.time = 0f;

        if (_globallyEnabled && !_userPausedGlobally)
        {
            _as.Play();
            SetToggleLabel(desactivar: true); // sigue siendo "desactivar" si está activo
        }
        // Si está desactivado, no cambia el estado del botón (permanece "activar")
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


    // UI Helpers

    private void SetToggleLabel(bool desactivar)
    {
        if (toggleButtonText == null) return;
        toggleButtonText.text = desactivar ? "Desactivar" : "Activar";
    }
}
