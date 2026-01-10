using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [Header("Bu Levelin Kuralları")]
    public float targetPercentage = 50f; // Bu levelin hedefi kaç?
    public float tolerance = 5f;         // Hata payı kaç?
    public int maxMoves = 3;             // Kaç hamle var?
}