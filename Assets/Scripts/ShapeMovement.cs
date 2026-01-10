using UnityEngine;

public class ShapeMovement : MonoBehaviour
{
    [Header("Rotation Settings")]
    public bool rotate = false;
    public float rotationSpeed = 50f;
    public bool randomizeRotationDirection = false;

    [Header("Movement Settings")]
    public bool move = false;
    public MovementType movementType = MovementType.SineWave;
    public float moveSpeed = 2f;
    public float moveRange = 3f;
    public bool moveVertical = false;

    [Header("Advanced Movement")]
    public bool useCustomPath = false;
    public Vector2[] pathPoints;
    private int currentPathIndex = 0;

    private Vector3 startPos;
    private float timeOffset;
    private float rotationDirection = 1f;

    public enum MovementType
    {
        SineWave,
        Linear,
        Circle,
        Custom
    }

    void Start()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, 100f);

        if (randomizeRotationDirection)
        {
            rotationDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
        }
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        if (rotate)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * rotationDirection * Time.deltaTime);
        }
    }

    void HandleMovement()
    {
        if (!move) return;

        switch (movementType)
        {
            case MovementType.SineWave:
                MoveSineWave();
                break;
            case MovementType.Linear:
                MoveLinear();
                break;
            case MovementType.Circle:
                MoveCircle();
                break;
            case MovementType.Custom:
                MoveCustomPath();
                break;
        }
    }

    void MoveSineWave()
    {
        float offset = Mathf.Sin((Time.time + timeOffset) * moveSpeed) * moveRange;

        if (moveVertical)
        {
            transform.position = startPos + new Vector3(0, offset, 0);
        }
        else
        {
            transform.position = startPos + new Vector3(offset, 0, 0);
        }
    }

    void MoveLinear()
    {
        float t = Mathf.PingPong((Time.time + timeOffset) * moveSpeed, 1f);
        float offset = Mathf.Lerp(-moveRange, moveRange, t);

        if (moveVertical)
        {
            transform.position = startPos + new Vector3(0, offset, 0);
        }
        else
        {
            transform.position = startPos + new Vector3(offset, 0, 0);
        }
    }

    void MoveCircle()
    {
        float angle = (Time.time + timeOffset) * moveSpeed;
        float x = Mathf.Cos(angle) * moveRange;
        float y = Mathf.Sin(angle) * moveRange;

        if (moveVertical)
        {
            transform.position = startPos + new Vector3(x, y, 0);
        }
        else
        {
            transform.position = startPos + new Vector3(x, 0, 0);
        }
    }

    void MoveCustomPath()
    {
        if (!useCustomPath || pathPoints == null || pathPoints.Length < 2)
        {
            MoveSineWave();
            return;
        }

        Vector2 targetPoint = pathPoints[currentPathIndex];
        Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.y);
        
        transform.position = Vector2.MoveTowards(currentPos2D, targetPoint, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(currentPos2D, targetPoint) < 0.1f)
        {
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (move && !useCustomPath)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = Application.isPlaying ? startPos : transform.position;

            if (movementType == MovementType.SineWave || movementType == MovementType.Linear)
            {
                if (moveVertical)
                {
                    Gizmos.DrawLine(center + Vector3.up * moveRange, center - Vector3.up * moveRange);
                }
                else
                {
                    Gizmos.DrawLine(center + Vector3.right * moveRange, center - Vector3.right * moveRange);
                }
            }
            else if (movementType == MovementType.Circle)
            {
                DrawCircleGizmo(center, moveRange);
            }
        }

        if (useCustomPath && pathPoints != null && pathPoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < pathPoints.Length; i++)
            {
                Vector3 point = pathPoints[i];
                Gizmos.DrawSphere(point, 0.2f);
                
                Vector3 nextPoint = pathPoints[(i + 1) % pathPoints.Length];
                Gizmos.DrawLine(point, nextPoint);
            }
        }
    }

    void DrawCircleGizmo(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}