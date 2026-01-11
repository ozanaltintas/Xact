using UnityEngine;

public class RandomSelfRotator : MonoBehaviour
{
    [Header("Dönüş Ayarları")]
    [Tooltip("Dönüşün yapılacağı eksen (Örn: Tekerlek için X=1, Y=0, Z=0)")]
    // DÜZELTME BURADA: Varsayılan değer Vector3.right (X ekseni) yapıldı.
    public Vector3 rotationAxis = Vector3.right; 

    [Header("Hız Aralığı (Derece/Saniye)")]
    [Tooltip("Minimum dönüş hızı")]
    public float minSpeed = 50f;
    [Tooltip("Maksimum dönüş hızı")]
    public float maxSpeed = 350f; // Tekerlek için hızı biraz artırdım, daha iyi görünür.

    [Header("Değişim Zamanlaması (Saniye)")]
    [Tooltip("Yön/Hız değişimi için en az beklenecek süre")]
    public float minChangeTime = 0.5f; // Daha sık değişim için süreyi biraz kıstım
    [Tooltip("Yön/Hız değişimi için en fazla beklenecek süre")]
    public float maxChangeTime = 2.0f;

    // --- Private (Gizli) Değişkenler ---
    private float currentSpeed;
    private float timeToNextChange;
    private int rotationDirection; // 1: Saat yönü, -1: Saat tersi

    void Start()
    {
        // Oyun başladığında ilk değerleri rastgele belirle
        PickNewValues();
    }

    void Update()
    {
        // Zamanlayıcıyı geri say
        timeToNextChange -= Time.deltaTime;

        // Eğer süre dolduysa yeni hız ve yön belirle
        if (timeToNextChange <= 0f)
        {
            PickNewValues();
        }

        ApplyRotation();
    }

    // Yeni rastgele değerler seçen yardımcı fonksiyon
    void PickNewValues()
    {
        // 1. Yeni rastgele bir hız belirle
        float randomBaseSpeed = Random.Range(minSpeed, maxSpeed);

        // 2. Rastgele bir yön belirle (Yazı tura atar gibi: %50 şans)
        // Random.value 0.0 ile 1.0 arasında bir sayı döndürür.
        rotationDirection = (Random.value > 0.5f) ? 1 : -1;

        // Hız ile yönü çarp (Örn: 100 * -1 = -100 hız)
        currentSpeed = randomBaseSpeed * rotationDirection;

        // 3. Bir sonraki değişim için zamanlayıcıyı rastgele kur
        timeToNextChange = Random.Range(minChangeTime, maxChangeTime);
    }

    // Dönüşü uygulayan fonksiyon
    void ApplyRotation()
    {
        // Hız * Geçen Zaman * Eksen Vektörü
        // Space.Self: Kendi yerel ekseni etrafında dönmesini sağlar.
        // rotationAxis.normalized: Vektörü 1 birimlik hale getirir.
        Vector3 rotationAmount = rotationAxis.normalized * currentSpeed * Time.deltaTime;
        transform.Rotate(rotationAmount, Space.Self);
    }
}