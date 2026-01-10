using UnityEngine;

public class InputController : MonoBehaviour
{
    [Header("Line Rendering")]
    private Vector2 startPos;
    private Vector2 endPos;
    private LineRenderer lineRenderer;
    public Color lineColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    public float lineWidth = 0.1f;
    public Material lineMaterial;

    [Header("Visual Feedback")]
    public GameObject sliceEffectPrefab;
    public float minSliceDistance = 0.5f;

    [Header("Audio")]
    public AudioClip sliceSound;
    private AudioSource audioSource;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = lineMaterial ?? new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.sortingOrder = 100;

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Hem mouse hem touch desteği
        bool isPressed = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        bool isHeld = Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary));
        bool isReleased = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        Vector2 inputPos = Input.touchCount > 0 ? 
            (Vector2)Input.GetTouch(0).position : 
            (Vector2)Input.mousePosition;

        if (isPressed)
        {
            // 2D için Vector3'ü Vector2'ye çeviriyoruz
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(inputPos);
            startPos = new Vector2(worldPos.x, worldPos.y);
            
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, startPos);
        }

        if (isHeld && lineRenderer.positionCount > 0)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(inputPos);
            Vector2 currentPos = new Vector2(worldPos.x, worldPos.y);
            lineRenderer.SetPosition(1, currentPos);
        }

        if (isReleased && lineRenderer.positionCount > 0)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(inputPos);
            endPos = new Vector2(worldPos.x, worldPos.y);
            lineRenderer.positionCount = 0;

            if (Vector2.Distance(startPos, endPos) >= minSliceDistance)
            {
                ExecuteSlice();
            }
        }
    }

    void ExecuteSlice()
    {
        GameObject[] sliceables = GameObject.FindGameObjectsWithTag("Sliceable");
        bool sliceHappened = false;

        foreach (GameObject obj in sliceables)
        {
            PolygonCollider2D col = obj.GetComponent<PolygonCollider2D>();
            
            if (col != null && DoesLineIntersectCollider(col, startPos, endPos))
            {
                try 
                {
                    SpriteSlicer.Slice(obj, startPos, endPos);
                    sliceHappened = true;

                    // Görsel efekt
                    if (sliceEffectPrefab != null)
                    {
                        Vector2 midPoint = (startPos + endPos) / 2f;
                        GameObject effect = Instantiate(sliceEffectPrefab, midPoint, Quaternion.identity);
                        Destroy(effect, 2f);
                    }

                    // Ses efekti
                    if (sliceSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(sliceSound);
                    }

                    // Haptic feedback (mobil titreşim)
                    #if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
                    #endif
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Kesme Hatası: " + e.Message);
                }
            }
        }

        if (sliceHappened)
        {
            if (GameLevelManager.Instance != null)
            {
                GameLevelManager.Instance.OnSliceExecuted();
            }
        }
    }

    bool DoesLineIntersectCollider(PolygonCollider2D col, Vector2 start, Vector2 end)
    {
        Vector2[] worldPoints = new Vector2[col.points.Length];
        for (int i = 0; i < col.points.Length; i++)
        {
            worldPoints[i] = col.transform.TransformPoint(col.points[i]);
        }

        for (int i = 0; i < worldPoints.Length; i++)
        {
            Vector2 p1 = worldPoints[i];
            Vector2 p2 = worldPoints[(i + 1) % worldPoints.Length];
            
            if (LineSegmentsIntersect(start, end, p1, p2))
                return true;
        }
        return false;
    }

    bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
        if (Mathf.Abs(d) < 0.0001f) return false;
        
        float u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        float v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;
        
        return (u >= 0 && u <= 1 && v >= 0 && v <= 1);
    }
}