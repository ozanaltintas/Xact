using UnityEngine;

/// <summary>
/// Alternatif kesim işlemleri için yardımcı sınıf
/// SpriteSlicer.cs ile aynı işi yapar, sadece farklı bir yaklaşım
/// </summary>
public class SlicerMachine : MonoBehaviour
{
    /// <summary>
    /// Yeni kesilen parçaları ayarlar (ana/atık ayrımı yapar)
    /// </summary>
    public static void SetupSlicedPiece(GameObject obj, Vector2 originalCenter)
    {
        // 1. Collider Ekle
        PolygonCollider2D col = obj.GetComponent<PolygonCollider2D>();
        if (col == null) col = obj.AddComponent<PolygonCollider2D>();

        // 2. Fizik Ekle
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = obj.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.linearDamping = 1f;
        rb.angularDamping = 1f;

        // 3. Ana parça mı yoksa atık mı kontrol et
        bool isMainBody = IsMainPiece(col, originalCenter);

        if (isMainBody)
        {
            // Ana parça
            obj.tag = "Sliceable";
            obj.name = "MainShape";

            // Yanlışlıkla eklenen DebrisDrifter varsa kaldır
            DebrisDrifter drifter = obj.GetComponent<DebrisDrifter>();
            if (drifter != null) Destroy(drifter);
        }
        else
        {
            // Atık parça
            obj.tag = "Untagged";
            obj.name = "Debris";

            // Drift efekti ekle
            if (obj.GetComponent<DebrisDrifter>() == null)
            {
                DebrisDrifter drifter = obj.AddComponent<DebrisDrifter>();
                
                // Rastgele yön
                drifter.driftDirection = Random.insideUnitCircle.normalized;
            }
        }
    }

    /// <summary>
    /// Bir parçanın ana parça olup olmadığını kontrol eder
    /// </summary>
    static bool IsMainPiece(PolygonCollider2D col, Vector2 originalCenter)
    {
        // Yöntem 1: Bounds kontrolü
        if (col.bounds.Contains(originalCenter))
            return true;

        // Yöntem 2: Overlap kontrolü (daha hassas)
        if (col.OverlapPoint(originalCenter))
            return true;

        // Yöntem 3: Alan kontrolü (ikisi de merkezi içermiyorsa büyük olanı seç)
        return false;
    }

    /// <summary>
    /// Polygon'un alanını hesaplar
    /// </summary>
    public static float CalculateArea(PolygonCollider2D poly)
    {
        if (poly.points == null || poly.points.Length < 3)
            return 0;

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

    /// <summary>
    /// İki parçadan hangisinin ana parça olduğunu belirler
    /// </summary>
    public static GameObject DetermineMainPiece(GameObject pieceA, GameObject pieceB, Vector2 originalCenter)
    {
        PolygonCollider2D colA = pieceA.GetComponent<PolygonCollider2D>();
        PolygonCollider2D colB = pieceB.GetComponent<PolygonCollider2D>();

        if (colA == null || colB == null)
            return pieceA;

        // Önce merkez kontrolü
        bool aContains = colA.OverlapPoint(originalCenter);
        bool bContains = colB.OverlapPoint(originalCenter);

        if (aContains && !bContains)
            return pieceA;
        if (bContains && !aContains)
            return pieceB;

        // İkisi de içeriyorsa veya hiçbiri içermiyorsa, büyük olanı seç
        float areaA = CalculateArea(colA);
        float areaB = CalculateArea(colB);

        return areaA >= areaB ? pieceA : pieceB;
    }

    /// <summary>
    /// Köprü metod - SpriteSlicer'ı çağırır
    /// </summary>
    public static void Slice(Vector2 start, Vector2 end)
    {
        GameObject[] sliceables = GameObject.FindGameObjectsWithTag("Sliceable");
        
        foreach (GameObject obj in sliceables)
        {
            SpriteSlicer.Slice(obj, start, end);
        }
    }
}