using System;
using System.Collections.Generic;
using UnityEngine;

public class RayCasting : MonoBehaviour
{
    public int RayCount;
    public int depth;
    private List<Color> colors;

    void Start()
    {
        colors.Add(Color.cyan);
        colors.Add(Color.black);
        colors.Add(Color.blue);
        colors.Add(Color.red);
        colors.Add(Color.yellow);
        colors.Add(Color.magenta);
        colors.Add(Color.green);
        
    }
    
    Vector2 CalculateRefractVector(Vector2 normal, Vector2 InDirection, float RIOR)
    {
        
        Vector2 vector2 = new Vector2();
        float foo = normal.y / normal.x;
        float foo1 =  InDirection.y / InDirection.x;
        foo = Mathf.Atan(foo);
        foo1 = Mathf.Atan(foo1);
        foo -= foo1;
        foo = Mathf.Sin(foo);
        foo *= RIOR;
        foo = Mathf.Asin(foo);
        foo1 = normal.y / normal.x;
        foo1 = Mathf.Atan(foo1);
        foo1 -= foo;
        if (normal.x <= 0)
        {
            vector2 = ConvertToDirection(foo1);
        }
        else
        {
            vector2 = ConvertToDirection(foo1);
            vector2 = new Vector2(-vector2.x, -vector2.y);
        }
        return vector2;
    }
    RaycastHit2D RefractRay(RaycastHit2D ray, Vector2 InDirection, int ReflectNum)
    {
        var LensScript = ray.collider.gameObject.GetComponent<LensScript>();
        float IndexOfRefraction = LensScript.IndexOfRefraction;
        //if (ReflectNum % 2 == 0) IndexOfRefraction = 1 / IndexOfRefraction;
        float RIOR = 1 / IndexOfRefraction;
        if (ReflectNum >= colors.Count-1) ReflectNum %= 6;
        Vector2 normal = ray.normal;
        Vector2 direction = CalculateRefractVector(normal, InDirection, RIOR);
        if (direction == new Vector2(float.NaN, float.NaN))
        {
            return new RaycastHit2D();
        }
        else
        {
            RaycastHit2D FooRayhit = hit(direction, ray.point);
            Debug.DrawRay(ray.point, direction*FooRayhit.distance, colors[ReflectNum]);
            return FooRayhit;
        }
    }
    RaycastHit2D hit(Vector2 direction, Vector2 StartPosition)
    {
        return Physics2D.Raycast(StartPosition, direction);
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
            Vector2 direction = ConvertToDirection(Mathf.PI*i/RayCount);
            var ray = hit(direction, transform.position);
            if (ray.collider)
            {
                int recursion = 0;
                Debug.DrawRay(transform.position,direction*ray.distance, Color.green);
                RaycastHit2D RefractedRay = RefractRay(ray, direction, recursion);
                //while (RefractedRay.collider && recursion < depth)
                //{
                //    RefractedRay = RefractRay(RefractedRay, direction, recursion);
                //    direction = GetDirection(transform.position, ray.point);
                //    recursion++;
                //}
            }
            else
            {
                Debug.DrawRay(transform.position,direction*100);
            }
        }
    }
        
}
