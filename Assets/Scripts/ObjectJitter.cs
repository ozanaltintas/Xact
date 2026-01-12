using UnityEngine;

public class ObjectJitter : MonoBehaviour
{
    [Header("Titreme Ayarları")]
    public float jitterAmount = 0.1f; // Ne kadar uzağa gidip geleceği (Mesafe)
    public float jitterSpeed = 20f;   // Titreme hızı

    [Header("Değişim Ayarları")]
    [Tooltip("Her kesildiğinde hızı bu aralıktaki rastgele bir sayıyla çarpar.")]
    public float minMultiplier = 0.8f; 
    public float maxMultiplier = 2.5f;

    private Vector3 initialLocalPosition;
    private float randomOffset;

    void Start()
    {
        // Nesnenin başlangıçtaki yerel pozisyonunu kaydet
        initialLocalPosition = transform.localPosition;

        // Her parça birbiriyle senkronize titremesin diye rastgele bir zaman farkı ekle
        randomOffset = Random.Range(0f, 100f);

        // --- HIZ DEĞİŞİMİ ---
        // Bu obje sahnede ilk oluştuğunda (veya kesilip yeni parça olduğunda)
        // hızı rastgele değiştir. Böylece her parça farklı delirir.
        jitterSpeed *= Random.Range(minMultiplier, maxMultiplier);
    }

    void Update()
    {
        // Sinüs dalgası ile sağa-sola (X ekseni) titreme hesapla
        float xOffset = Mathf.Sin((Time.time + randomOffset) * jitterSpeed) * jitterAmount;

        // Pozisyonu güncelle (Y ve Z sabit kalır, sadece X titrer)
        // İstersen yOffset ekleyerek yukarı aşağı da titretebilirsin.
        transform.localPosition = initialLocalPosition + new Vector3(xOffset, 0f, 0f);
    }
}