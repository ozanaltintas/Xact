using UnityEngine;

public class SliceEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public Color particleColor = Color.white;
    public int particleCount = 20;
    public float particleSpeed = 5f;
    public float particleLifetime = 0.5f;
    public float particleSize = 0.1f;

    [Header("Flash Settings")]
    public bool useFlash = true;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    private ParticleSystem ps;

    void Start()
    {
        CreateParticleSystem();
        
        if (useFlash)
        {
            CreateFlashEffect();
        }

        Destroy(gameObject, particleLifetime + 0.5f);
    }

    void CreateParticleSystem()
    {
        ps = gameObject.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = particleSpeed;
        main.startSize = particleSize;
        main.startColor = particleColor;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] 
        { 
            new ParticleSystem.Burst(0f, particleCount) 
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] 
            { 
                new GradientColorKey(particleColor, 0f), 
                new GradientColorKey(particleColor, 1f) 
            },
            new GradientAlphaKey[] 
            { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
    }

    void CreateFlashEffect()
    {
        GameObject flashObj = new GameObject("Flash");
        flashObj.transform.position = transform.position;
        
        SpriteRenderer sr = flashObj.AddComponent<SpriteRenderer>();
        
        Texture2D tex = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = flashColor;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        sr.sprite = sprite;
        sr.color = flashColor;
        sr.sortingOrder = 1000;

        FlashFade fade = flashObj.AddComponent<FlashFade>();
        fade.duration = flashDuration;
    }
}

public class FlashFade : MonoBehaviour
{
    public float duration = 0.1f;
    private float elapsed = 0f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
        
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;

        float scale = Mathf.Lerp(0.1f, 2f, elapsed / duration);
        transform.localScale = Vector3.one * scale;

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}