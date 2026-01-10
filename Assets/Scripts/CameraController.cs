using UnityEngine;
using System.Collections;

/// <summary>
/// Kesim anında kamera shake ve zoom efektleri
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;
    public float shakeDecay = 1f;

    [Header("Zoom Settings")]
    public bool useZoomEffect = true;
    public float zoomAmount = 0.5f;
    public float zoomDuration = 0.2f;

    private Vector3 originalPosition;
    private float originalSize;
    private Camera cam;
    private bool isShaking = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        originalPosition = transform.localPosition;
        
        if (cam != null)
        {
            originalSize = cam.orthographicSize;
        }
    }

    /// <summary>
    /// Kamera sarsıntısı başlat
    /// </summary>
    public void ShakeCamera()
    {
        if (!isShaking)
        {
            StartCoroutine(Shake());
        }
    }

    /// <summary>
    /// Kamera sarsıntısı başlat (özel parametrelerle)
    /// </summary>
    public void ShakeCamera(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(Shake(duration, magnitude));
        }
    }

    IEnumerator Shake(float duration = -1, float magnitude = -1)
    {
        if (duration < 0) duration = shakeDuration;
        if (magnitude < 0) magnitude = shakeMagnitude;

        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            magnitude *= (1f - shakeDecay * Time.deltaTime);

            yield return null;
        }

        transform.localPosition = originalPosition;
        isShaking = false;
    }

    /// <summary>
    /// Zoom in-out efekti
    /// </summary>
    public void ZoomEffect()
    {
        if (useZoomEffect && cam != null)
        {
            StartCoroutine(Zoom());
        }
    }

    IEnumerator Zoom()
    {
        float targetSize = originalSize - zoomAmount;
        float elapsed = 0f;

        // Zoom in
        while (elapsed < zoomDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (zoomDuration / 2f);
            cam.orthographicSize = Mathf.Lerp(originalSize, targetSize, t);
            yield return null;
        }

        elapsed = 0f;

        // Zoom out
        while (elapsed < zoomDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (zoomDuration / 2f);
            cam.orthographicSize = Mathf.Lerp(targetSize, originalSize, t);
            yield return null;
        }

        cam.orthographicSize = originalSize;
    }

    /// <summary>
    /// Kombine efekt: Shake + Zoom
    /// </summary>
    public void SliceEffect()
    {
        ShakeCamera();
        ZoomEffect();
    }

    /// <summary>
    /// Kamerayı hedefe takip ettir (smooth follow)
    /// </summary>
    public void FollowTarget(Transform target, float smoothSpeed = 5f)
    {
        StartCoroutine(SmoothFollow(target, smoothSpeed));
    }

    IEnumerator SmoothFollow(Transform target, float smoothSpeed)
    {
        while (target != null)
        {
            Vector3 desiredPosition = target.position;
            desiredPosition.z = transform.position.z;
            
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            yield return null;
        }
    }

    /// <summary>
    /// Belirli bir pozisyona yumuşak geçiş
    /// </summary>
    public void MoveToPosition(Vector3 targetPosition, float duration)
    {
        StartCoroutine(SmoothMove(targetPosition, duration));
    }

    IEnumerator SmoothMove(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        targetPosition.z = startPosition.z;
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }

    /// <summary>
    /// Kamerayı sıfırla
    /// </summary>
    public void ResetCamera()
    {
        StopAllCoroutines();
        transform.localPosition = originalPosition;
        if (cam != null)
        {
            cam.orthographicSize = originalSize;
        }
        isShaking = false;
    }
}