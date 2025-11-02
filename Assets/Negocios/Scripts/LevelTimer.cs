using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    [Tooltip("Duración del nivel en minutos (ej: 5.0f para 5 minutos)")]
    public float levelDurationMinutes = 5f;

    [Header("Referencia UI")]
    [Tooltip("Asigna aquí el TextMeshProUGUI para mostrar el tiempo")]
    public TextMeshProUGUI timerText;

    [Header("Audio")]
    [Tooltip("Audio de facto")]
    public AudioSource factoVoice;

    [Header("Canva")]
    [Tooltip("Canva que se muestrar cuando se acaba el tiempo")]
    public Canvas canvaOnTimeUp;

    [Tooltip("Mensaje a mostrar en consola si se acaba el tiempo.")]
    public string timeUpMessage = "¡Tiempo agotado! Reiniciando el nivel.";

    private float _maxTimeSeconds;
    private float _currentTime;
    private bool _isRunning = false;
    private bool _isPaused = false;
    private bool _timeUp = false;

    void Start()
    {
        // 1. Inicia al cargar el nivel (escena)
        _maxTimeSeconds = levelDurationMinutes * 60f;
        _currentTime = _maxTimeSeconds;
        _isRunning = true;
        _isPaused = false;
        _timeUp = false;
        UpdateTimerUI();
    }

    void Update()
    {
        if (!_isRunning || _isPaused || _timeUp)
        {
            return;
        }

        // Cuenta regresiva
        _currentTime -= Time.deltaTime;

        // Actualiza la UI cada frame
        UpdateTimerUI();

        // 4. Se acaba el tiempo
        if (_currentTime <= 0f)
        {
            _currentTime = 0f;
            _timeUp = true;
            OnTimeUp();
        }
    }

    // 3. Método para pausar (se llama desde GestorValidacionGlobal)
    public void PauseTimer()
    {
        if (_timeUp) return; // No pausar si ya se agotó

        _isPaused = true;
        Debug.Log("⏱️ Temporizador pausado. Nivel completado.");
    }

    // Método para reanudar el tiempo (útil si el usuario falla después de completar)
    public void ResumeTimer()
    {
        _isPaused = false;
        Debug.Log("⏱️ Temporizador reanudado.");
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        // Formato MM:SS
        int minutes = Mathf.FloorToInt(_currentTime / 60f);
        int seconds = Mathf.FloorToInt(_currentTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Opcional: cambiar color al finalizar
        if (_currentTime < 10f)
        {
            timerText.color = Color.red;
        }
    }

    private void OnTimeUp()
    {
        _isRunning = false;
        Debug.Log(timeUpMessage);

        // Ejecuta la acción al acabarse el tiempo
        if (canvaOnTimeUp != null)
        {
            timerText.gameObject.SetActive(false);
            canvaOnTimeUp.gameObject.SetActive(true);
            factoVoice.mute = true;
        }
    }

    public void ReloadScene() {
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }
}
