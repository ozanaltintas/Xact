using UnityEngine;

public class SlicerMachine : MonoBehaviour
{
    // Bu metod kesilen YENİ parçalara özellik atamak için kullanılır
    public static void SetupSlicedPiece(GameObject obj)
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

        // 3. KRİTİK AYRIM: Bu parça merkezde mi, yoksa kenar süsü mü?
        // Oyunun merkezi (0,0) kabul edilir.
        // Eğer bu parça (0,0) noktasını kapsıyorsa ANA GÖVDEDİR.
        
        // Bounds (Sınırlar) kontrolü yapıyoruz:
        bool isMainBody = col.bounds.Contains(Vector3.zero);

        // Alternatif hassas kontrol (Eğer şekil yamuksa):
        // bool isMainBody = col.OverlapPoint(Vector2.zero);

        if (isMainBody)
        {
            // Bu ana parça. Kesilmeye devam edilebilir ve Puana dahildir.
            obj.tag = "Sliceable";
            
            // Eğer yanlışlıkla DebrisDrifter eklendiyse kaldır (Ana parça uçmasın)
            DebrisDrifter drifter = obj.GetComponent<DebrisDrifter>();
            if (drifter != null) Destroy(drifter);
        }
        else
        {
            // Bu atık parça. Puan hesaplamasına KATILMAMALI.
            obj.tag = "Untagged"; // Tag'i değiştirdik ki Manager bunu toplamasın!
            
            // Atık parça olduğu için ekrandan süzülüp gitsin
            if (obj.GetComponent<DebrisDrifter>() == null)
                obj.AddComponent<DebrisDrifter>();
        }
    }
    
    // Köprü Metod
    public static void Slice(Vector2 start, Vector2 end)
    {
        // SpriteSlicer işlemleri buraya (InputController hallediyor zaten)
    }
}