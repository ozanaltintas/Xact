using UnityEngine;

public class UI_JitterEffect : MonoBehaviour
{
    [Header("Titreme Ayarları")]
    [Tooltip("Titremenin şiddeti. Minimal olması için küçük değerler girin (Örn: UI için 2-5 arası, 3D obje için 0.05-0.1 arası).")]
    public float jitterAmount = 3f; 

    [Tooltip("Titremenin hızı. Ne kadar yüksek olursa o kadar 'sinirli' titrer.")]
    public float jitterSpeed = 25f;

    [Tooltip("Efekti açıp kapatmak için.")]
    public bool isEnabled = true;

    // Objenin başladığı orijinal yerel pozisyonu
    private Vector3 initialLocalPosition;
    // Rastgelelik için kullanılan zaman sayacı
    private float randomTimeSeed;

    void Start()
    {
        // Oyun başladığında objenin o anki yerel pozisyonunu "merkez" olarak kaydet.
        // Bu sayede obje titrerken olduğu yerden uzaklaşmaz.
        initialLocalPosition = transform.localPosition;

        // Her obje aynı anda aynı yöne titremesin diye rastgele bir başlangıç zamanı seç.
        randomTimeSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!isEnabled)
        {
            // Efekt kapalıysa objeyi orijinal merkezine döndür ve çık.
            transform.localPosition = initialLocalPosition;
            return;
        }

        ApplyJitter();
    }

    void ApplyJitter()
    {
        // Titreme hareketi oluşturmak için Perlin Noise kullanıyoruz.
        // Perlin Noise, rastgele ama birbiriyle yumuşak geçişli sayılar üretir (0 ile 1 arası).
        // Time.time * jitterSpeed ile zaman içinde bu gürültü haritasında ilerliyoruz.
        float noiseValue = Mathf.PerlinNoise(randomTimeSeed + Time.time * jitterSpeed, 0f);

        // Noise değeri 0 ile 1 arasındadır. Bunu -1 ile 1 arasına çekelim ki
        // hem sağa hem sola eşit miktarda gidebilsin.
        float normalizedNoise = (noiseValue * 2f) - 1f; // Sonuç: -1 civarı ile +1 civarı

        // Hesaplanan yönü şiddetle çarp
        float offsetX = normalizedNoise * jitterAmount;

        // Yeni pozisyonu hesapla: Orijinal merkez + sadece X eksenindeki sapma
        Vector3 newPosition = initialLocalPosition;
        newPosition.x += offsetX;

        // Pozisyonu uygula
        transform.localPosition = newPosition;
    }
    
    // Eğer obje başka bir script tarafından hareket ettirilirse (örn: ekrana giriş animasyonu)
    // bu fonksiyonu çağırarak yeni merkez noktasını güncelleyebilirsiniz.
    public void ResetInitialPosition()
    {
        initialLocalPosition = transform.localPosition;
    }
}