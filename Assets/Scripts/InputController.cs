using UnityEngine;

public class InputController : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 endPos;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lineRenderer.SetPosition(1, currentPos);
        }

        if (Input.GetMouseButtonUp(0))
        {
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lineRenderer.positionCount = 0;
            ExecuteSlice();
        }
    }

    void ExecuteSlice()
    {
        GameObject[] sliceables = GameObject.FindGameObjectsWithTag("Sliceable");
        bool sliceHappened = false;

        foreach (GameObject obj in sliceables)
        {
            PolygonCollider2D col = obj.GetComponent<PolygonCollider2D>();
            
            // Eğer objenin collider'ı varsa ve bizim çizdiğimiz çizgi collider'a değiyorsa:
            if (col != null && col.OverlapPoint(startPos) == false && col.OverlapPoint(endPos) == false)
            {
                // NOT: Bu satır projendeki "SpriteSlicer" scriptini kullanır.
                // Eğer hata alırsan SpriteSlicer.cs dosyanın içeriğini bana atman gerekecek.
                
                // SpriteSlicer'ın genelde static bir Slice metodu vardır:
                // Dönüş değeri olarak yeni parçaları verir veya void döner.
                // En yaygın kullanım şeklini yazıyorum:
                
                try 
                {
                    // VARSAYIM: SpriteSlicer.Slice(GameObject, Vector2 start, Vector2 end)
                    SpriteSlicer.Slice(obj, startPos, endPos);
                    
                    // Eğer kod buraya kadar hatasız geldiyse kesim başarılıdır
                    sliceHappened = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Kesme Hatası: " + e.Message);
                    Debug.LogError("SpriteSlicer scriptinle benim yazdığım kod uyuşmuyor!");
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
}