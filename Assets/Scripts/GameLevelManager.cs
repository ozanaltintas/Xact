using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullanıyorsun
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

    private float targetPercentage;
    private int remainingMoves = 3;
    private float initialTotalArea;
    private bool isLevelEnding = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int index)
    {
        isLevelEnding = false;
        if(messageText != null) messageText.text = ""; 

        // 1. Temizlik
        StopAllCoroutines(); // Eski hesaplamalar varsa durdur
        GameObject[] leftovers = GameObject.FindGameObjectsWithTag("Sliceable");
        foreach (GameObject obj in leftovers)
        {
            Destroy(obj);
        }

        if (index >= levelPrefabs.Count)
        {
            if(messageText != null) messageText.text = "OYUN BİTTİ! TEBRİKLER!";
            return;
        }

        // 2. Yeni Cismi Yarat (Biraz bekleyip alanı hesapla)
        StartCoroutine(SpawnAndCalculate(index));
    }

    // Cismin yaratılmasını ve ilk alan hesabını garantiye alan coroutine
    IEnumerator SpawnAndCalculate(int index)
    {
        // Eski objelerin yok olması için bir kare bekle
        yield return null; 

        GameObject newShape = Instantiate(levelPrefabs[index], spawnPoint.position, Quaternion.identity);
        
        LevelSettings settings = newShape.GetComponent<LevelSettings>();
        if (settings != null)
        {
            targetPercentage = settings.targetPercentage;
            if(targetPercentageText != null) targetPercentageText.text = "Target: %" + targetPercentage.ToString("F1");
        }
        else
        {
            targetPercentage = 50f;
            if(targetPercentageText != null) targetPercentageText.text = "Target: %50.0";
        }

        // Fizik motorunun objeyi oturtması için bir kare daha bekle
        yield return null;

        initialTotalArea = CalculateTotalArea();
        remainingMoves = 3;
        UpdateUI(100f);
    }

    // InputController'dan çağrılan metod
    public void OnSliceExecuted()
    {
        if (isLevelEnding) return;

        remainingMoves--;
        
        // HESAPLAMAYI HEMEN YAPMA! Bir sonraki kareye bırak.
        StartCoroutine(CalculateDelayed());
    }

    // Köprü metod
    public void OnCutFinished()
    {
        OnSliceExecuted();
    }

    IEnumerator CalculateDelayed()
    {
        // Unity'nin 'Destroy' edilen objeleri sahneden silmesi için bekle
        yield return null; 
        yield return null; // Garanti olsun diye 2 kare bekliyoruz (Physics update cycle)

        float currentArea = CalculateTotalArea();
        float percentage = 0f;
        
        if (initialTotalArea > 0)
            percentage = (currentArea / initialTotalArea) * 100f;
        
        // Yüzde eksi çıkamaz veya saçma bir şekilde artamaz, güvenlik kontrolü:
        if (percentage > 100f && remainingMoves < 3) 
        {
            // Eğer kesmemize rağmen %100'den büyükse, eski parça silinmemiş demektir.
            // Bu durumda oyuncuya o anki hatalı değeri göstermek yerine max 100 göster.
            // Ama 2 kare beklediğimiz için buraya düşme ihtimali çok düşük.
            percentage = 100f; 
        }
        
        UpdateUI(percentage);
        CheckResult(percentage);
    }

    void CheckResult(float currentPercent)
    {
        float diff = Mathf.Abs(currentPercent - targetPercentage);

        if (diff <= 5f)
        {
            StartCoroutine(LevelSuccessRoutine());
        }
        else if (remainingMoves <= 0)
        {
            StartCoroutine(LevelFailRoutine());
        }
    }

    IEnumerator LevelSuccessRoutine()
    {
        isLevelEnding = true;
        if(messageText != null) 
        {
            messageText.text = "MÜKEMMEL!";
            messageText.color = Color.green;
        }

        yield return new WaitForSeconds(1.5f);

        currentLevelIndex++;
        LoadLevel(currentLevelIndex);
    }

    IEnumerator LevelFailRoutine()
    {
        isLevelEnding = true;
        if(messageText != null)
        {
            messageText.text = "BAŞARISIZ!";
            messageText.color = Color.red;
        }

        yield return new WaitForSeconds(1.5f);

        LoadLevel(currentLevelIndex);
    }

    void UpdateUI(float currentPercent)
    {
        if(movesText != null) movesText.text = "Moves: " + remainingMoves;
        if(currentPercentageText != null) currentPercentageText.text = "Current: %" + currentPercent.ToString("F1");
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
            // Sadece 'Sliceable' olanları topla. 
            // SlicerMachine.cs scriptindeki mantık sayesinde atık parçalar 'Untagged' oluyor,
            // bu yüzden burada toplanmıyorlar.
            if (col.gameObject.CompareTag("Sliceable"))
            {
                total += GetArea(col);
            }
        }
        return total;
    }

    float GetArea(PolygonCollider2D poly)
    {
        if(poly.points == null || poly.points.Length < 3) return 0;

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
}