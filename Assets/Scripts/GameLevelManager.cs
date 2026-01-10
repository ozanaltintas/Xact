using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance;

    [Header("Level Settings")]
    public Transform spawnPoint;
    public List<GameObject> levelPrefabs;
    private int currentLevelIndex = 0;

    [Header("UI Elements")]
    public TMP_Text targetPercentageText;
    public TMP_Text currentPercentageText;
    public TMP_Text movesText;
    public TMP_Text messageText;
    public TMP_Text levelText;

    [Header("UI Panels")]
    public GameObject levelCompletePanel;
    public GameObject levelFailPanel;

    [Header("Audio")]
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip sliceSound;
    private AudioSource audioSource;

    [Header("Visual Effects")]
    public ParticleSystem confettiEffect;
    
    [Header("Color Settings")]
    [Tooltip("İşaretli değilse cisimlerin rengi değişmez, senin ayarladığın gibi kalır.")]
    public bool useColorFeedback = false; 
    
    public Color farColor = Color.red;    
    public Color closeColor = Color.yellow; 
    public Color perfectColor = Color.green; 

    private float targetPercentage;
    private int remainingMoves = 3;
    private float initialTotalArea;
    private bool isLevelEnding = false;
    private GameObject currentShape;
    private GameObject currentShapeObj; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailPanel != null) levelFailPanel.SetActive(false);
    }

    public void LoadLevel(int index)
    {
        isLevelEnding = false;
        
        if (messageText != null) 
        {
            messageText.text = "";
        }

        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailPanel != null) levelFailPanel.SetActive(false);

        StopAllCoroutines();
        
        GameObject[] leftovers = GameObject.FindGameObjectsWithTag("Sliceable");
        foreach (GameObject obj in leftovers)
        {
            Destroy(obj);
        }

        GameObject[] debris = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in debris)
        {
            if (obj.GetComponent<DebrisDrifter>() != null)
                Destroy(obj);
        }

        if (index >= levelPrefabs.Count)
        {
            ShowGameComplete();
            return;
        }

        StartCoroutine(SpawnAndCalculate(index));
    }

    IEnumerator SpawnAndCalculate(int index)
    {
        yield return null;

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point atanmamış!");
            yield break;
        }

        currentShape = Instantiate(levelPrefabs[index], spawnPoint.position, Quaternion.identity);
        currentShape.tag = "Sliceable";
        currentShapeObj = currentShape; 

        if (useColorFeedback)
        {
            UpdateShapeColor(100f, 50f); 
        }

        LevelSettings settings = currentShape.GetComponent<LevelSettings>();
        if (settings != null)
        {
            targetPercentage = settings.targetPercentage;
            remainingMoves = settings.maxMoves;
        }
        else
        {
            targetPercentage = 50f;
            remainingMoves = 3;
        }

        if (levelText != null)
            levelText.text = $"Level {index + 1}";

        if (targetPercentageText != null)
            targetPercentageText.text = $"🎯 {targetPercentage:F0}%";

        yield return null;

        initialTotalArea = CalculateTotalArea();
        
        if (initialTotalArea <= 0)
        {
            Debug.LogError("Başlangıç alanı hesaplanamadı!");
            yield break;
        }

        UpdateUI(100f);
        
        if (useColorFeedback)
        {
            UpdateShapeColor(100f, targetPercentage);
        }
    }

    public void OnSliceExecuted()
    {
        if (isLevelEnding) return;

        remainingMoves--;

        if (sliceSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sliceSound);
        }

        StartCoroutine(CalculateDelayed());
    }

    public void OnCutFinished()
    {
        OnSliceExecuted();
    }

    IEnumerator CalculateDelayed()
    {
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(ProcessSliceResult());
    }

    IEnumerator ProcessSliceResult()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Sliceable");
        
        if (pieces.Length > 0)
        {
            GameObject biggestPiece = null;
            float maxArea = -1f;

            foreach (GameObject obj in pieces)
            {
                PolygonCollider2D col = obj.GetComponent<PolygonCollider2D>();
                if (col != null)
                {
                    float area = GetArea(col);
                    if (area > maxArea)
                    {
                        maxArea = area;
                        biggestPiece = obj;
                    }
                }
            }

            foreach (GameObject obj in pieces)
            {
                if (obj == biggestPiece)
                {
                    DebrisDrifter drifter = obj.GetComponent<DebrisDrifter>();
                    if (drifter != null) Destroy(drifter);
                    currentShapeObj = obj;
                }
                else
                {
                    obj.tag = "Untagged";
                    DebrisDrifter drifter = obj.GetComponent<DebrisDrifter>();
                    if (drifter != null) drifter.enabled = true;
                }
            }
        }

        float currentArea = CalculateTotalArea();
        float percentage = 0f;

        if (initialTotalArea > 0)
            percentage = (currentArea / initialTotalArea) * 100f;

        percentage = Mathf.Clamp(percentage, 0f, 100f);

        UpdateUI(percentage);
        
        if (useColorFeedback)
        {
            UpdateShapeColor(percentage, targetPercentage);
        }

        CheckResult(percentage);
        yield return null;
    }

    void UpdateShapeColor(float current, float target)
    {
        if (currentShapeObj == null) return;

        SpriteRenderer sr = currentShapeObj.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float diff = Mathf.Abs(current - target);

        if (diff <= 5f)
        {
            sr.color = perfectColor; 
        }
        else if (diff < 25f)
        {
            float t = 1 - (diff - 5f) / 20f; 
            sr.color = Color.Lerp(closeColor, perfectColor, t);
        }
        else
        {
            float t = 1 - (diff - 25f) / 50f;
            sr.color = Color.Lerp(farColor, closeColor, Mathf.Clamp01(t));
        }
    }

    void CheckResult(float currentPercent)
    {
        float diff = Mathf.Abs(currentPercent - targetPercentage);

        // 1. Durum: Hedef tuttuysa (Hamle bitmese bile KAZANIR)
        if (diff <= 5f)
        {
            StartCoroutine(LevelSuccessRoutine(currentPercent));
        }
        // 2. Durum: Hamle bitti ve hedef tutmadıysa (KAYBEDER)
        else if (remainingMoves <= 0)
        {
            StartCoroutine(LevelFailRoutine(currentPercent));
        }
        // 3. Durum: Hamle var ve hedef tutmadı -> Oyuna devam
    }

    IEnumerator LevelSuccessRoutine(float finalPercent)
    {
        isLevelEnding = true;

        // --- YENİ EKLENEN KISIM: BEKLEME SÜRESİ ---
        // Kesim bittikten sonra sonucu göstermeden önce 0.5 saniye bekle
        yield return new WaitForSeconds(0.5f); 
        // ------------------------------------------

        if (messageText != null)
        {
            messageText.text = GetSuccessMessage(Mathf.Abs(finalPercent - targetPercentage));
        }

        if (successSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        // Paneli gösterdikten sonra bir sonraki level için bekleme
        yield return new WaitForSeconds(2f);

        currentLevelIndex++;
        LoadLevel(currentLevelIndex);
    }

    IEnumerator LevelFailRoutine(float finalPercent)
    {
        isLevelEnding = true;

        // --- YENİ EKLENEN KISIM: BEKLEME SÜRESİ ---
        // Başarısız olmadan önce de oyuncunun durumu kavraması için bekle
        yield return new WaitForSeconds(0.5f);
        // ------------------------------------------

        if (messageText != null)
        {
            float diff = Mathf.Abs(finalPercent - targetPercentage);
            messageText.text = $"BAŞARISIZ!\n({diff:F1}% fark)";
        }

        if (failSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(failSound);
        }

        if (levelFailPanel != null)
        {
            levelFailPanel.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        LoadLevel(currentLevelIndex);
    }

    string GetSuccessMessage(float diff)
    {
        if (diff <= 1f) return "MÜKEMMEL! 🌟";
        if (diff <= 3f) return "HARİKA! ⭐";
        return "BAŞARILI! ✓";
    }

    void ShowGameComplete()
    {
        if (messageText != null)
        {
            messageText.text = "OYUNU TAMAMLADIN!\nTEBRİKLER! 🎉";
        }

        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }
    }

    void UpdateUI(float currentPercent)
    {
        if (movesText != null)
        {
            movesText.text = $"✂️ {remainingMoves}";
        }

        if (currentPercentageText != null)
        {
            currentPercentageText.text = $"{currentPercent:F1}%";
        }
    }

    float CalculateTotalArea()
    {
        float total = 0;

#if UNITY_2023_1_OR_NEWER
        PolygonCollider2D[] colliders = FindObjectsByType<PolygonCollider2D>(FindObjectsSortMode.None);
#else
        PolygonCollider2D[] colliders = FindObjectsOfType<PolygonCollider2D>();
#endif

        foreach (var col in colliders)
        {
            if (col.gameObject.CompareTag("Sliceable"))
            {
                total += GetArea(col);
            }
        }
        return total;
    }

    float GetArea(PolygonCollider2D poly)
    {
        if (poly.points == null || poly.points.Length < 3) return 0;

        float area = 0;
        Vector2[] points = poly.points;
        Vector3 scale = poly.transform.lossyScale;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 p1 = Vector2.Scale(points[i], scale);
            Vector2 p2 = Vector2.Scale(points[(i + 1) % points.Length], scale);
            area += (p1.x * p2.y) - (p2.x * p1.y);
        }
        return Mathf.Abs(area) / 2f;
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void NextLevel()
    {
        currentLevelIndex++;
        LoadLevel(currentLevelIndex);
    }
}