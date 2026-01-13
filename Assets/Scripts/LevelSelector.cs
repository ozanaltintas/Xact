using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelSelector : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject levelButtonPrefab; 
    public Transform contentPanel;       

    [Header("Renkler")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = Color.gray;

    void Start()
    {
        GenerateLevelButtons();
    }

    void OnEnable()
    {
        GenerateLevelButtons();
    }

    void GenerateLevelButtons()
    {
        if (contentPanel == null) return;
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        if (GameLevelManager.Instance == null)
        {
            Debug.LogError("GameLevelManager bulunamadı!");
            return;
        }

        int totalLevels = GameLevelManager.Instance.levelPrefabs.Count;
        int reachedLevel = PlayerPrefs.GetInt("SavedLevelIndex", 0); 

        for (int i = 0; i < totalLevels; i++)
        {
            int levelIndex = i; // Geçici değişken (Closure sorunu olmasın diye)
            GameObject btnObj = Instantiate(levelButtonPrefab, contentPanel);
            Button btn = btnObj.GetComponent<Button>();
            
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = (levelIndex + 1).ToString();

            Transform lockIcon = btnObj.transform.Find("LockIcon");

            if (levelIndex <= reachedLevel)
            {
                // AÇIK BÖLÜM
                btn.interactable = true;
                btn.image.color = unlockedColor;
                if (lockIcon != null) lockIcon.gameObject.SetActive(false);

                // --- DÜZELTME BURADA ---
                // Tıklayınca doğru index ile LoadLevel çağıracak
                btn.onClick.AddListener(() => LoadLevel(levelIndex));
            }
            else
            {
                // KİLİTLİ BÖLÜM
                btn.interactable = false; 
                btn.image.color = lockedColor;
                if (lockIcon != null) lockIcon.gameObject.SetActive(true);
            }
        }
    }

    void LoadLevel(int index)
    {
        // 1. Doğrudan seçilen bölümü yükle
        GameLevelManager.Instance.LoadLevel(index);

        // 2. UIManager'daki 'StartGame' fonksiyonunu ÇAĞIRMIYORUZ.
        // Çünkü o fonksiyon gider kayıtlı leveli tekrar yükler.
        // Onun yerine sadece paneli değiştiriyoruz:
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameplay(); // Sadece oyun ekranını aç
            Time.timeScale = 1f;               // Oyunu devam ettir (Pause ise çözülsün)
        }
    }
}