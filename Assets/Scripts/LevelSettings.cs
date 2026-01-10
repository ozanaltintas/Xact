using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [Header("Level Rules")]
    [Tooltip("Hedef alan yüzdesi (örn: 50 = %50)")]
    [Range(10f, 90f)]
    public float targetPercentage = 50f;

    [Tooltip("Hata payı (örn: 5 = ±%5)")]
    [Range(1f, 10f)]
    public float tolerance = 5f;

    [Tooltip("Kaç kesme hakkı var?")]
    [Range(1, 10)]
    public int maxMoves = 3;

    [Header("Difficulty Info")]
    [Tooltip("Sadece bilgi amaçlı - otomatik hesaplanır")]
    public DifficultyLevel difficulty;

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Expert
    }

    void OnValidate()
    {
        CalculateDifficulty();
    }

    void CalculateDifficulty()
    {
        float score = 0;

        if (targetPercentage <= 30f || targetPercentage >= 70f)
            score += 2;
        else if (targetPercentage <= 40f || targetPercentage >= 60f)
            score += 1;

        if (maxMoves <= 2)
            score += 2;
        else if (maxMoves == 3)
            score += 1;

        if (tolerance <= 3f)
            score += 2;
        else if (tolerance <= 5f)
            score += 1;

        ShapeMovement movement = GetComponent<ShapeMovement>();
        if (movement != null)
        {
            if (movement.rotate && movement.move)
                score += 3;
            else if (movement.rotate || movement.move)
                score += 1;
        }

        if (score <= 2)
            difficulty = DifficultyLevel.Easy;
        else if (score <= 4)
            difficulty = DifficultyLevel.Medium;
        else if (score <= 6)
            difficulty = DifficultyLevel.Hard;
        else
            difficulty = DifficultyLevel.Expert;
    }

    public bool IsSuccessful(float achievedPercentage)
    {
        float diff = Mathf.Abs(achievedPercentage - targetPercentage);
        return diff <= tolerance;
    }

    public string GetFeedback(float achievedPercentage)
    {
        float diff = Mathf.Abs(achievedPercentage - targetPercentage);
        
        if (diff <= tolerance * 0.2f)
            return "MÜKEMMEL!";
        else if (diff <= tolerance * 0.5f)
            return "ÇOK İYİ!";
        else if (diff <= tolerance)
            return "BAŞARILI!";
        else if (diff <= tolerance * 1.5f)
            return "YAKLAŞTIN!";
        else
            return "TEKRAR DENE!";
    }
}