#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// LevelSettings component'i için özel Inspector görünümü
/// Editor klasörüne koymanıza gerek yok, bu script otomatik çalışır
/// </summary>
[CustomEditor(typeof(LevelSettings))]
public class LevelSettingsEditor : Editor
{
    private SerializedProperty targetPercentageProp;
    private SerializedProperty toleranceProp;
    private SerializedProperty maxMovesProp;

    void OnEnable()
    {
        targetPercentageProp = serializedObject.FindProperty("targetPercentage");
        toleranceProp = serializedObject.FindProperty("tolerance");
        maxMovesProp = serializedObject.FindProperty("maxMoves");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        LevelSettings settings = (LevelSettings)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Hedef Yüzde
        EditorGUILayout.PropertyField(targetPercentageProp);
        EditorGUILayout.HelpBox($"Oyuncunun ulaşması gereken alan: %{settings.targetPercentage:F1}", MessageType.Info);

        EditorGUILayout.Space();

        // Tolerans
        EditorGUILayout.PropertyField(toleranceProp);
        EditorGUILayout.HelpBox($"Kabul edilebilir hata payı: ±%{settings.tolerance:F1}", MessageType.Info);

        EditorGUILayout.Space();

        // Hamle Sayısı
        EditorGUILayout.PropertyField(maxMovesProp);
        EditorGUILayout.HelpBox($"Oyuncunun {settings.maxMoves} kesme hakkı var", MessageType.Info);

        EditorGUILayout.Space();

        // Zorluk Göstergesi
        string difficultyText = GetDifficultyInfo(settings);
        MessageType difficultyType = GetDifficultyMessageType(settings.difficulty);
        
        EditorGUILayout.HelpBox(difficultyText, difficultyType);

        // ShapeMovement kontrolü
        ShapeMovement movement = settings.GetComponent<ShapeMovement>();
        if (movement != null)
        {
            EditorGUILayout.Space();
            string movementInfo = "Bu level HAREKETLÄ° bir ÅŸekil kullanÄ±yor:\n";
            if (movement.rotate) movementInfo += "• Rotasyon aktif\n";
            if (movement.move) movementInfo += "• Hareket aktif\n";
            EditorGUILayout.HelpBox(movementInfo, MessageType.Warning);
        }

        // Test Aralığı Göstergesi
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Başarı Aralığı:", EditorStyles.boldLabel);
        float min = settings.targetPercentage - settings.tolerance;
        float max = settings.targetPercentage + settings.tolerance;
        EditorGUILayout.LabelField($"Minimum: %{min:F1}");
        EditorGUILayout.LabelField($"Hedef: %{settings.targetPercentage:F1}");
        EditorGUILayout.LabelField($"Maximum: %{max:F1}");

        // Test butonu
        EditorGUILayout.Space();
        if (GUILayout.Button("Test Bu Level'i", GUILayout.Height(30)))
        {
            TestLevel(settings);
        }

        serializedObject.ApplyModifiedProperties();
    }

    string GetDifficultyInfo(LevelSettings settings)
    {
        return $"Zorluk: {GetDifficultyName(settings.difficulty)}\n{GetDifficultyDescription(settings.difficulty)}";
    }

    string GetDifficultyName(LevelSettings.DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case LevelSettings.DifficultyLevel.Easy: return "KOLAY";
            case LevelSettings.DifficultyLevel.Medium: return "ORTA";
            case LevelSettings.DifficultyLevel.Hard: return "ZOR";
            case LevelSettings.DifficultyLevel.Expert: return "UZMAN";
            default: return "BİLİNMEYEN";
        }
    }

    string GetDifficultyDescription(LevelSettings.DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case LevelSettings.DifficultyLevel.Easy:
                return "Yeni başlayanlar için ideal.";
            case LevelSettings.DifficultyLevel.Medium:
                return "Dengeli zorluk seviyesi.";
            case LevelSettings.DifficultyLevel.Hard:
                return "Deneyimli oyuncular için.";
            case LevelSettings.DifficultyLevel.Expert:
                return "Sadece uzmanlar için!";
            default:
                return "";
        }
    }

    MessageType GetDifficultyMessageType(LevelSettings.DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case LevelSettings.DifficultyLevel.Easy:
                return MessageType.Info;
            case LevelSettings.DifficultyLevel.Medium:
                return MessageType.None;
            case LevelSettings.DifficultyLevel.Hard:
                return MessageType.Warning;
            case LevelSettings.DifficultyLevel.Expert:
                return MessageType.Error;
            default:
                return MessageType.None;
        }
    }

    void TestLevel(LevelSettings settings)
    {
        if (Application.isPlaying)
        {
            // Oyun çalışıyorsa bu level'e geç
            if (GameLevelManager.Instance != null)
            {
                Debug.Log($"Testing Level: {settings.gameObject.name}");
                // Bu level'in index'ini bul ve load et
                // GameLevelManager'da index sistemine göre ayarlaman gerekebilir
            }
        }
        else
        {
            Debug.Log($"Level Test Ä°statistikleri:\n" +
                     $"Hedef: %{settings.targetPercentage:F1}\n" +
                     $"Tolerans: ±%{settings.tolerance:F1}\n" +
                     $"Hamle: {settings.maxMoves}\n" +
                     $"Zorluk: {settings.difficulty}");
        }
    }
}
#endif