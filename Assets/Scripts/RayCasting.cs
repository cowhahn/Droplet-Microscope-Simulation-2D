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
    public bool DrawReflection;
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
            direction = ConvertToDirection(Math.PI*i/RayCount);
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
    Vector2 CalculateRefractVector(Vector2 normal, Vector2 ray, double IOR, RaycastHit2D rayhit)
    {
        double theta = Vector2.Angle(normal, ray);
        double signTheta = Math.Sign(Vector2.SignedAngle(normal, ray));
        if (Math.Abs(theta) >= 90) theta = 180 - theta;
        //Debug.Log("Theta 0: " + theta * signTheta);
        theta = theta * Math.PI / 180;
        theta = Math.Sin(theta);
        theta *= IOR;
        theta = Math.Asin(theta);
        if (theta > Math.PI / 2 && DrawReflection) return Vector2.Reflect(ray, normal);
        if (theta > Math.PI / 2 && !DrawReflection) return new Vector2();
        //Debug.Log("Theta 1: " + theta * 180 / Mathf.PI);
        Vector2 oppositeNormal = new Vector2(-normal.x, -normal.y);
        double thetaNormal = Vector2.Angle(normal, Vector2.right);
        thetaNormal = thetaNormal * Math.PI / 180;
        if (signTheta == -1)
        {
            return ConvertToDirection(thetaNormal + theta);
        }
        else
        {
            return ConvertToDirection(thetaNormal - theta); 
        }
    }
    RaycastHit2D RefractRay(RaycastHit2D ray, Vector2 InDirection, int RefractNum)
    {
        bool InsideObject = false;
        if (RefractNum % 2 == 0) InsideObject = true;
        var LensScript = ray.collider.gameObject.GetComponent<LensScript>();
        Vector2 normal = ray.normal;
        double IndexOfRefraction;
        if (!InsideObject)
        {
            IndexOfRefraction = 1/LensScript.IndexOfRefraction;
            LastIndexOfRefraction = LensScript.IndexOfRefraction;
        }
        else
        {
            IndexOfRefraction = LastIndexOfRefraction;
        }
        Vector2 RefractDirection = CalculateRefractVector(normal, InDirection, IndexOfRefraction, ray);
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
    Vector2 ConvertToDirection(double theta)
    {
        double xdir = Math.Cos(theta);
        double ydir = Math.Sin(theta);
        return new Vector2((float)xdir, (float)ydir);
    }

    Vector2 GetDirection(Vector2 StartPoint, Vector2 Endpoint)
    {
        double xdist = Endpoint.x - StartPoint.x;
        double ydist = Endpoint.y - StartPoint.y;
        return new Vector2((float)xdist, (float)ydist);
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