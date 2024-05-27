using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LensScript : MonoBehaviour
{
    public float radius;
    public int PointAmount;
    public float Offest;
    public float IndexOfRefraction;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    EdgeCollider2D collider;

    Vector2[] DrawCirclePoints(float radius, int PointAmount) 
    {
        Vector2[] Points = new Vector2[PointAmount];
        float DeltaTheta = 2 * Mathf.PI / PointAmount;
        for (int i = 0;  i < PointAmount; i++)
        {
            float xPos = Mathf.Cos(i * DeltaTheta);
            float yPos = Mathf.Sin(i * DeltaTheta);
            Vector2 Point = new Vector2(xPos*radius,yPos*radius);
            Points[i] = Point;
        }
        return Points;
    }
    float DistanceBetween(Vector2 point1, Vector2 point2) 
    {
        float xDistance = point1.x - point2.x;
        float yDistance = point1.y - point2.y;
        xDistance *= xDistance;
        yDistance *= yDistance;
        float distance  = xDistance + yDistance;
        return (float)Math.Sqrt(distance);
    }
    Vector2[] CheckIfInsideCircle(Vector2[] circle0, Vector2[] circle1) 
    {
        bool OutsideCircle = false;
        List<Vector2> vector2s = new List<Vector2>();
        for (int i = 0; i < circle0.Length; i++)
        {
            OutsideCircle = false;
            foreach (Vector2 point in circle1) 
            {
                if (DistanceBetween(circle0[i], point) >= (2 * radius)) 
                {
                    OutsideCircle = true;
                    break;
                }
            }
            if (!OutsideCircle) 
            {
                vector2s.Add(circle0[i]);
            }        
        }
        return vector2s.ToArray();
    }
    int[] CreateTriangles(Vector3[] vector3s, Vector2[] lens0) 
    {
        List<int> ints = new List<int>();
        for (int i = 0; i < lens0.Length; i++) 
        {
            ints.Add(i);
            ints.Add(i + 1);
            ints.Add(0);
            ints.Add(0);
            ints.Add(i + lens0.Length - 1);
            ints.Add(i + lens0.Length);
        }
        return ints.ToArray();
    }
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        collider = GetComponent<EdgeCollider2D>();

        Vector2[] EdgePoints0 = DrawCirclePoints(radius, PointAmount);
        Vector2[] EdgePoints1 = DrawCirclePoints(radius, PointAmount);

        for (int i = 0; i < EdgePoints1.Length; i++) EdgePoints1[i] += new Vector2(0, Offest);

        Vector2[] lens0 = CheckIfInsideCircle(EdgePoints0, EdgePoints1);
        Vector2[] lens1 = CheckIfInsideCircle(EdgePoints1, EdgePoints0);

        List<Vector2> lens = new List<Vector2>();
        lens.AddRange(lens0);
        lens.AddRange(lens1);
        lens.Reverse();

        Vector3[] Vector3Vertices = new Vector3[lens.Count];

        for (int i = 0; i < lens.Count; i++)
        {
            Vector3Vertices[i] = new Vector3(lens[i].x, lens[i].y, 1);
        }
        mesh.Clear();
        mesh.vertices = Vector3Vertices;
        mesh.triangles = CreateTriangles(Vector3Vertices, lens0);
        mesh.RecalculateNormals();

        transform.position = new Vector3(0,-Offest/2,0);
        collider.points = lens.ToArray();

    }
}
