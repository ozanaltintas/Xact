using UnityEngine;

public class ShapeMovement : MonoBehaviour
{
    [Header("Rotation Settings")]
    public bool rotate = false;
    public float rotationSpeed = 50f; // Saniyedeki derece

    [Header("Movement Settings")]
    public bool move = false;
    public float moveSpeed = 2f;
    public float moveRange = 3f; // Ne kadar uzağa gidip gelecek
    public bool moveVertical = false; // İşaretliyse yukarı-aşağı, değilse sağa-sola

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Dönme Mantığı
        if (rotate)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        // Hareket Mantığı (PingPong)
        if (move)
        {
            float offset = Mathf.Sin(Time.time * moveSpeed) * moveRange;
            
            if (moveVertical)
            {
                transform.position = startPos + new Vector3(0, offset, 0);
            }
            else
            {
                transform.position = startPos + new Vector3(offset, 0, 0);
            }
        }
    }
}