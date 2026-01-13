using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject levelCompletePanel;
    public GameObject levelFailPanel;
    public GameObject levelPickPanel; // YENİ EKLENDİ

    [Header("Buttons")]
    public Button playButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button settingsButton;
    public Button homeButton;
    public Button pickLevelButton;     // YENİ EKLENDİ (Menüdeki buton)
    public Button levelPickBackButton; // YENİ EKLENDİ (Paneldeki geri butonu)

    [Header("Settings UI")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle vibrateToggle;

    [Header("Gameplay UI")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public Image progressBar;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupButtons();
        LoadSettings();
        ShowMainMenu();
    }

    void SetupButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
        
        if (homeButton != null)
            homeButton.onClick.AddListener(GoToMainMenu);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        // --- YENİ EKLENEN BUTON BAĞLANTILARI ---
        if (pickLevelButton != null)
            pickLevelButton.onClick.AddListener(ShowLevelPick);

        if (levelPickBackButton != null)
            levelPickBackButton.onClick.AddListener(HideLevelPick);
    }

    void LoadSettings()
    {
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        if (vibrateToggle != null)
            vibrateToggle.isOn = PlayerPrefs.GetInt("Vibrate", 1) == 1;
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void ShowGameplay()
    {
        HideAllPanels();
        if (gameplayPanel != null)
            gameplayPanel.SetActive(true);
    }

    public void ShowLevelComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            PlayButtonSound();
        }
    }

    public void ShowLevelFail()
    {
        if (levelFailPanel != null)
        {
            levelFailPanel.SetActive(true);
            PlayButtonSound();
        }
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            PlayButtonSound();
        }
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            PlayButtonSound();
        }
    }

    // --- YENİ FONKSİYONLAR ---
    public void ShowLevelPick()
    {
        HideAllPanels(); // Diğer her şeyi kapat
        if (levelPickPanel != null)
        {
            levelPickPanel.SetActive(true); // Sadece bunu aç
            PlayButtonSound();
        }
    }

    public void HideLevelPick()
    {
        if (levelPickPanel != null)
        {
            levelPickPanel.SetActive(false);
        }
        ShowMainMenu(); // Geri basınca ana menüye dön
        PlayButtonSound();
    }
    // -------------------------

    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailPanel != null) levelFailPanel.SetActive(false);
        if (levelPickPanel != null) levelPickPanel.SetActive(false); // Bunu da listeye ekledik
    }

    public void StartGame()
    {
        ShowGameplay();
        Time.timeScale = 1f;
        PlayButtonSound();
        
        if (GameLevelManager.Instance != null)
        {
            GameLevelManager.Instance.ContinueFromSavedLevel();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null)
            pausePanel.SetActive(true);
        PlayButtonSound();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        PlayButtonSound();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        if (GameLevelManager.Instance != null)
        {
            GameLevelManager.Instance.RestartLevel();
        }
        HideAllPanels();
        ShowGameplay();
        PlayButtonSound();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        ShowMainMenu();
        PlayButtonSound();
    }

    public void QuitGame()
    {
        PlayButtonSound();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = progress;
        }
    }
}