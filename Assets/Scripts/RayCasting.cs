using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RayCasting : MonoBehaviour
{
    public int RayCount;
    public int depth;
    public bool DrawNoHits = true;
    public bool ParallelRays = false;
    public float width;
    private float LastIndexOfRefraction;
    private RaycastHit2D ray;
    private Vector2 pos;
    private Color[] colors = 
    { 
        Color.cyan, 
        Color.green, 
        Color.blue, 
        Color.black, 
        Color.magenta, 
        Color.red, 
        Color.yellow 
    };

    Vector2 InitialCast(int i)
    {
        Vector2 direction = new Vector2();
        if (!ParallelRays)
        {
            direction = ConvertToDirection(Mathf.PI*i/RayCount);
            pos = transform.position;
            ray = hit(direction, transform.position);
        }
        else
        {
            direction = Vector2.up;
            float step = width * i / RayCount;
            pos = transform.position;
            pos = new Vector2(pos.x - (width / 2) + step, pos.y);
            ray = hit(direction, pos);
        }
        return direction;
    }
    RaycastHit2D hit(Vector2 direction, Vector2 StartPosition)
    {
        return Physics2D.Raycast(StartPosition, direction);
    }
    Vector2 CalculateRefractVector(Vector2 normal, Vector2 ray, float IOR, bool InsideObject)
    {
        bool rayUp = false;
        Vector2 normalSigns = new Vector2(Mathf.Sign(normal.x), Mathf.Sign(normal.y));
        Vector2 raySigns = new Vector2(Mathf.Sign(ray.x), Mathf.Sign(ray.y));
        normal = normal.normalized;
        ray = ray.normalized;
        normal = new Vector2(Mathf.Abs(normal.x), Mathf.Abs(normal.y));
        ray = new Vector2(Mathf.Abs(ray.x), Mathf.Abs(ray.y));
        float theta = (ray.x * normal.y) + (normal.x * ray.y);
        theta = Mathf.Asin(theta);
        if (ray.x == 0)
        {
            theta = Mathf.Abs(theta);
            rayUp = true;
        }
        if (raySigns.x == raySigns.y && normalSigns.x == normalSigns.y && !rayUp) theta -= 1;
        if (raySigns.x != raySigns.y && normalSigns.x != normalSigns.y && !rayUp) theta -= 1;
        theta = Mathf.Sin(theta);
        theta *= IOR;
        float theta2 = Mathf.Asin(theta);
        Debug.Log(theta2 * 180 / Math.PI );
        if (theta2 >= 0)
        {
            Debug.Log("Type 0");
            float thetaRay = Mathf.Asin(normal.y);
            thetaRay += theta2;
            if (normalSigns.x > 0) thetaRay = Mathf.PI - thetaRay;
            Vector2 outputDirection = ConvertToDirection(thetaRay);             
            return new Vector2(outputDirection.x, outputDirection.y);
        }
        else
        {
            Debug.Log("Type 1");
            float thetaRay = Mathf.Asin(normal.y);
            thetaRay += theta2;
            if (normalSigns.x < 0) thetaRay = Mathf.PI - thetaRay;
            Vector2 outputDirection = ConvertToDirection(thetaRay);           
            return new Vector2(-outputDirection.x, outputDirection.y);
        }
    }

    Vector2 CalculateReflectVector(Vector2 normal, Vector2 InDirection)
    {
        return new Vector2();
    }
    RaycastHit2D RefractRay(RaycastHit2D ray, Vector2 InDirection, int RefractNum)
    {
        bool InsideObject = false;
        if (RefractNum % 2 == 0) InsideObject = true;
        var LensScript = ray.collider.gameObject.GetComponent<LensScript>();
        Vector2 normal = ray.normal;
        float IndexOfRefraction;
        Debug.DrawRay(ray.point,ray.normal, Color.red);
        Debug.DrawRay(ray.point,-ray.normal, Color.yellow);
        if (!InsideObject)
        {
            IndexOfRefraction = 1/LensScript.IndexOfRefraction;
            LastIndexOfRefraction = LensScript.IndexOfRefraction;
        }
        else
        {
            IndexOfRefraction = LastIndexOfRefraction;
        }
        Vector2 RefractDirection = CalculateRefractVector(normal, InDirection, IndexOfRefraction, InsideObject);
        if (RefractDirection == new Vector2(float.NaN, float.NaN))
        {
            RefractDirection = CalculateReflectVector(normal, InDirection);
        }
        Vector2 offset = RefractDirection / 1000;
        var RefractRay = hit(RefractDirection, ray.point + offset);
        if (RefractRay.collider)
        {
            Debug.DrawRay(ray.point,RefractDirection*RefractRay.distance,Color.black);
            return RefractRay;

        }
        else
        {
            Debug.DrawRay(ray.point, RefractDirection*100, Color.red);
            return RefractRay;
        }
    }
    Vector2 ConvertToDirection(float theta)
    {
        float xdir = Mathf.Cos(theta);
        float ydir = Mathf.Sin(theta);
        return new Vector2(xdir, ydir);
    }

    Vector2 GetDirection(Vector2 StartPoint, Vector2 Endpoint)
    {
        float xdist = Endpoint.x - StartPoint.x;
        float ydist = Endpoint.y - StartPoint.y;
        return new Vector2(xdist, ydist);
    }
    void Update()
    {
        for (int i = 0; i < RayCount; i++)
        {
            Vector2 direction = InitialCast(i);
            if (ray.collider)
            {
                int recursion = 1;
                Vector2 tempdirection = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
                Vector2 normalabs = new Vector2(Mathf.Abs(ray.normal.x), Mathf.Abs(ray.normal.y));
                Debug.DrawRay(pos,direction*ray.distance, Color.green);
                //Debug.DrawRay(ray.point,ray.normal, Color.red);
                //Debug.DrawRay(ray.point,-ray.normal, Color.yellow);
                //Debug.Log("Normal Direction: " + ray.normal);
                //Debug.Log("Ray Direction: " + direction);
                var RefractingRay = RefractRay(ray, direction, recursion);
                while (RefractingRay.collider && recursion < depth)
                {
                    recursion++;
                    direction = GetDirection(ray.point, RefractingRay.point);
                    RefractingRay = RefractRay(RefractingRay, direction, recursion);
                }
            }
            else
            {
                if(DrawNoHits) Debug.DrawRay(pos,direction*100);
            }
        }
    }
        
}