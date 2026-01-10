using UnityEngine;

public class DebrisDrifter : MonoBehaviour
{
    [Header("Movement")]
    public Vector2 driftDirection;
    public float speed = 2f;
    public float rotationSpeed = 50f;

    [Header("Fade Effect")]
    public bool useFadeEffect = true;
    public float fadeStartTime = 1f;
    public float fadeDuration = 2f;

    private float lifeTime = 0f;
    private Renderer rend;
    private Color originalColor;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        rend = GetComponent<Renderer>();
        if (rend != null && useFadeEffect)
        {
            propBlock = new MaterialPropertyBlock();
            rend.GetPropertyBlock(propBlock);
            originalColor = rend.material.color;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.angularVelocity = Random.Range(-rotationSpeed, rotationSpeed);
        }

        Destroy(gameObject, 5f);
    }

    void Update()
    {
        lifeTime += Time.deltaTime;

        transform.Translate(driftDirection * speed * Time.deltaTime, Space.World);

        if (useFadeEffect && rend != null && lifeTime >= fadeStartTime)
        {
            float fadeProgress = (lifeTime - fadeStartTime) / fadeDuration;
            fadeProgress = Mathf.Clamp01(fadeProgress);
            
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
            
            propBlock.SetColor("_Color", newColor);
            rend.SetPropertyBlock(propBlock);

            if (fadeProgress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}