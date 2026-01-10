using UnityEngine;

public class DebrisDrifter : MonoBehaviour
{
    public Vector2 driftDirection; // Gideceği yön
    public float speed = 2f;       // Hızı

    void Start()
    {
        // 1. Çarpışmayı Kapat (Hayalet Modu)
        // Böylece ana parçaya veya diğer engellere takılmaz
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // 2. Güvenlik için 5 saniye sonra kesin yok et (ekranda sıkışırsa diye)
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // Her karede belirlenen yöne doğru yavaşça ilerle
        transform.Translate(driftDirection * speed * Time.deltaTime, Space.World);
    }

    // Ekran dışına çıktığı an yok et
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}