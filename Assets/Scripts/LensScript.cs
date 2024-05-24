using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LensScript : MonoBehaviour
{
    public float radius;
    public int PointAmount;
    public float Offest;
    public Sprite CircleSprite;
    public float scale;
    public float deltaY = .2f;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

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
    void CreateCircle(Vector2 loc, int inst) 
    {
        GameObject circle = new GameObject();
        circle.name = "circle"+inst;
        circle.transform.position = loc;
        circle.transform.localScale = new Vector3(scale,scale,scale);
        circle.AddComponent<SpriteRenderer>();
        SpriteRenderer m_SpriteRenderer = circle.GetComponent<SpriteRenderer>();
        m_SpriteRenderer.sprite = CircleSprite;
        m_SpriteRenderer.color = Color.white;

    }
    Vector2[] PopulateMesh(Vector2[] lens, Vector2[] lens0, Vector2[] lens1) 
    {
        List<Vector2> vector2s = new List<Vector2>();
        Vector2[] lens1Reversed = new Vector2[lens1.Length];
        lens1Reversed = lens1.Reverse().ToArray();
        for (int i = 0; i < lens1.Length; i++)
        {
            float distance = lens0[i].y - lens1[i].y;
            int num = (int)(distance/deltaY);
            for (int j = 0; j < num; j++)
            {
                vector2s.Add(new Vector2(lens0[i].x, lens0[i].y - (j * deltaY)));
            }
        }
        vector2s.AddRange(lens0);
        vector2s.AddRange(lens1);
        vector2s.Reverse();
        return vector2s.ToArray();
    }
    int FindIndexOfAbove(Vector3[] vector3s, int index) 
    {
        int IndexOfClosest = 0;
        float LastDist = float.MaxValue;
        vector3s.ToList();
        for (int i = 0; i < vector3s.Length; i++)
        {
            if (vector3s[i].x - vector3s[index].x == 0 && vector3s[i].y - vector3s[index].y < LastDist) 
            {
                IndexOfClosest = i;
                LastDist = vector3s[i].y - vector3s[index].y;
            }
        }
        return IndexOfClosest;
    }
    int FindIndexOfBelow(Vector3[] vector3s, int index)
    {
        int IndexOfClosest = 0;
        float LastDist = float.MaxValue;
        vector3s.ToList();
        for (int i = 0; i < vector3s.Length; i++)
        {
            if (vector3s[i].x - vector3s[index].x == 0 && vector3s[index]. y - vector3s[i].y < LastDist)
            {
                IndexOfClosest = i;
                LastDist = vector3s[index].y - vector3s[i].y;
            }
        }
        return IndexOfClosest;

    }
    int[] CreateTriangles(Vector3[] vector3s, Vector2[] lens0) 
    {
        List<int> ints = new List<int>();
        for (int i = 0; i < lens0.Length; i++) 
        {
            ints.Add(i);
            ints.Add(i + 1);
            ints.Add(FindIndexOfAbove(vector3s, i + 1));
            ints.Add(i + lens0.Length);
            ints.Add(i + lens0.Length+1);
            ints.Add(FindIndexOfBelow(vector3s, i + lens0.Length + 1));
        }
        return ints.ToArray();
    }
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        Vector2[] EdgePoints0 = DrawCirclePoints(radius, PointAmount);
        Vector2[] EdgePoints1 = DrawCirclePoints(radius, PointAmount);

        for (int i = 0; i < EdgePoints1.Length; i++) EdgePoints1[i] += new Vector2(0, Offest);

        Vector2[] lens0 = CheckIfInsideCircle(EdgePoints0, EdgePoints1);
        Vector2[] lens1 = CheckIfInsideCircle(EdgePoints1, EdgePoints0);

        Vector2[] lens = new Vector2[lens0.Length + lens1.Length];

        for (int i = 0; i < lens0.Length; i++)
        { 
            lens[i] = lens0[i];
            lens[i+lens0.Length] = lens1[i];

        }

        Vector2[] Vector2Vertices = PopulateMesh(lens, lens0, lens1);
        Vector3[] Vector3Vertices = new Vector3[Vector2Vertices.Length];

        for (int i = 0; i < Vector2Vertices.Length; i++)
        {
            Vector3Vertices[i] = new Vector3(Vector2Vertices[i].x, Vector2Vertices[i].y, 1);
        }
        int count = 0;
        foreach (Vector2 loc in Vector2Vertices)
        {
            CreateCircle(loc, count);
            count++;
        }
        mesh.Clear();
        mesh.vertices = Vector3Vertices;
        mesh.triangles = CreateTriangles(Vector3Vertices, lens0);
        mesh.RecalculateNormals();
    }
}
