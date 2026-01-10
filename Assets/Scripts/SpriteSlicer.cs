using UnityEngine;
using System.Collections.Generic;

public class SpriteSlicer
{
    public static void Slice(GameObject target, Vector2 lineStart, Vector2 lineEnd)
    {
        PolygonCollider2D collider = target.GetComponent<PolygonCollider2D>();
        if (collider == null) return;

        Vector2 originalCenter = target.transform.position;

        Vector2[] worldPoints = new Vector2[collider.points.Length];
        for (int i = 0; i < collider.points.Length; i++)
        {
            worldPoints[i] = target.transform.TransformPoint(collider.points[i]);
        }

        List<Vector2> partA_World = new List<Vector2>();
        List<Vector2> partB_World = new List<Vector2>();

        for (int i = 0; i < worldPoints.Length; i++)
        {
            Vector2 p1 = worldPoints[i];
            Vector2 p2 = worldPoints[(i + 1) % worldPoints.Length];

            bool isP1Right = IsRightOfLine(lineStart, lineEnd, p1);
            bool isP2Right = IsRightOfLine(lineStart, lineEnd, p2);

            if (isP1Right) partA_World.Add(p1);
            else partB_World.Add(p1);

            if (isP1Right != isP2Right)
            {
                Vector2 intersection = GetIntersection(lineStart, lineEnd, p1, p2);
                if (intersection != Vector2.positiveInfinity)
                {
                    partA_World.Add(intersection);
                    partB_World.Add(intersection);
                }
            }
        }

        if (partA_World.Count > 2 && partB_World.Count > 2)
        {
            GameObject pieceA = CreatePiece(target, partA_World, "Piece_A");
            GameObject pieceB = CreatePiece(target, partB_World, "Piece_B");

            GameObject mainPiece = null;
            GameObject debrisPiece = null;

            if (IsPointInPolygon(originalCenter, partA_World))
            {
                mainPiece = pieceA;
                debrisPiece = pieceB;
            }
            else
            {
                mainPiece = pieceB;
                debrisPiece = pieceA;
            }

            mainPiece.tag = "Sliceable"; 
            mainPiece.name = "MainShape";

            debrisPiece.tag = "Untagged"; 
            debrisPiece.name = "Debris";
            
            DebrisDrifter drifter = debrisPiece.AddComponent<DebrisDrifter>();
            
            Vector3 mainCenter = mainPiece.GetComponent<Renderer>().bounds.center;
            Vector3 debrisCenter = debrisPiece.GetComponent<Renderer>().bounds.center;
            drifter.driftDirection = (debrisCenter - mainCenter).normalized;

            Object.Destroy(target);
        }
    }

    static GameObject CreatePiece(GameObject original, List<Vector2> worldPoints, string name)
    {
        GameObject piece = new GameObject(name);
        piece.transform.position = original.transform.position;
        piece.transform.rotation = original.transform.rotation;
        piece.transform.localScale = Vector3.one;

        Vector2[] localPoints = new Vector2[worldPoints.Count];
        for (int i = 0; i < worldPoints.Count; i++)
        {
            localPoints[i] = piece.transform.InverseTransformPoint(worldPoints[i]);
        }

        PolygonCollider2D poly = piece.AddComponent<PolygonCollider2D>();
        poly.points = localPoints;
        
        Rigidbody2D rb = piece.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; 
        rb.linearDamping = 1f;

        MeshFilter meshFilter = piece.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = piece.AddComponent<MeshRenderer>();
        
        MeshRenderer originalRenderer = original.GetComponent<MeshRenderer>();
        if(originalRenderer) 
            meshRenderer.material = originalRenderer.material;
        else 
        {
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
            SpriteRenderer sr = original.GetComponent<SpriteRenderer>();
            if(sr) meshRenderer.material.color = sr.color;
        }

        Mesh mesh = poly.CreateMesh(false, false);
        meshFilter.mesh = mesh;

        return piece;
    }

    static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int intersectCount = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 p1 = polygon[i];
            Vector2 p2 = polygon[(i + 1) % polygon.Count];
            if (((p1.y > point.y) != (p2.y > point.y)) &&
                (point.x < (p2.x - p1.x) * (point.y - p1.y) / (p2.y - p1.y) + p1.x))
            {
                intersectCount++;
            }
        }
        return (intersectCount % 2) == 1;
    }

    static bool IsRightOfLine(Vector2 a, Vector2 b, Vector2 p)
    {
        return ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)) < 0;
    }

    static Vector2 GetIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
        if (Mathf.Abs(d) < 0.0001f) return Vector2.positiveInfinity;
        float u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        return new Vector2(p1.x + u * (p2.x - p1.x), p1.y + u * (p2.y - p1.y));
    }
}