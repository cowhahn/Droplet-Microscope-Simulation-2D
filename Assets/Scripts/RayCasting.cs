using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

public class RayCasting : MonoBehaviour
{
    
    public int RayCount;
    public int depth;
    public bool DrawNoHits = true;
    public bool ParallelRays = false;
    public bool DrawReflection;
    public bool DrawVirtualRays;
    public float width;
    public float xavg;
    public float yavg;
    public float ydev;
    public float xdev;
    public float focalPoint;
    public float outputAngle;
    public float lineWidth;
    public List<double> LSA = new List<double>();
    public List<double> TSA = new List<double>();
    
    private float LastIndexOfRefraction;
    private RaycastHit2D ray;
    private Vector2 pos;
    private List<Vector2[]> rays = new List<Vector2[]>();
    private float startAngle;
    private List<string> _lineNames = new List<string>();
    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    private int lineCount = 0;
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

    void LineManager(Vector2 p1, Vector2 p2, int i, Color color)
    {
        if (!_lineNames.Contains("Line " + i))
        {
            GameObject line = new GameObject();
            line.name = "Line " + i;
            line.AddComponent<LineRenderer>();
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.SetPositions(new Vector3[] {p1, p2});
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            _lineNames.Add("Line " + i);
            _lineRenderers.Add(lineRenderer);
        }
        else
        {
            int index = _lineNames.IndexOf("Line " + i);
            _lineRenderers[index].SetPositions(new Vector3[] {p1, p2});
            _lineRenderers[index].startWidth = lineWidth;
            _lineRenderers[index].endWidth = lineWidth;
            _lineRenderers[index].startColor = color;
            _lineRenderers[index].endColor = color;
            
        }
    }
    
    Vector2 InitialCast(int i)
    {
        Vector2 direction = new Vector2();
        if (!ParallelRays)
        {
            direction = ConvertToDirection(outputAngle * i / RayCount + startAngle);
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
        Vector2 dir = new Vector2();
        double theta = Vector2.Angle(normal, ray);
        double signTheta = Math.Sign(Vector2.SignedAngle(normal, ray));
        //Debug.DrawRay(rayhit.point, normal, colors[2]);
        //Debug.DrawRay(rayhit.point, -normal, colors[3]);
        if (Math.Abs(theta) >= 90) theta = 180 - theta;
        //Debug.Log("Theta 0: " + theta * signTheta);
        theta = theta * Math.PI / 180;
        //Debug.Log(theta);
        theta = Math.Sin(theta);
        theta *= IOR;
        theta = Math.Asin(theta);
        //Debug.Log(theta);
        if (theta > Math.PI / 2 && DrawReflection) return Vector2.Reflect(ray, normal);
        if (theta > Math.PI / 2 && !DrawReflection) return new Vector2();
        //Debug.Log("Theta 1: " + theta * 180 / Mathf.PI);
        Vector2 oppositeNormal = new Vector2(-normal.x, -normal.y);
        double thetaNormal = Vector2.Angle(normal, Vector2.right);
        thetaNormal = thetaNormal * Math.PI / 180;
        if ((thetaNormal < theta && signTheta == 1) || (thetaNormal > theta && signTheta == -1))
        {
            dir = ConvertToDirection(thetaNormal - theta);
            dir = new Vector2(-dir.x, dir.y);
        }
        else
        {
            dir = ConvertToDirection(thetaNormal + theta);
            dir = new Vector2(-dir.x, dir.y);
        }
        return dir;
    }
    RaycastHit2D RefractRay(RaycastHit2D ray, Vector2 InDirection, int RefractNum)
    {
        bool InsideObject = false;
        if (RefractNum % 2 != 0) InsideObject = true;
        var LensScript = ray.collider.gameObject.GetComponent<LensScript>();
        Vector2 normal = ray.normal;
        double IndexOfRefraction;
        if (!InsideObject)
        {
            IndexOfRefraction = LastIndexOfRefraction;
            //Debug.Log("Inside");
        }
        else
        {
            //Debug.Log("Outside");
            IndexOfRefraction = 1/LensScript.IndexOfRefraction;
            LastIndexOfRefraction = LensScript.IndexOfRefraction;
        }
        //Debug.Log(IndexOfRefraction);
        Vector2 RefractDirection = CalculateRefractVector(normal, InDirection, IndexOfRefraction, ray);
        Vector2 offset = RefractDirection / 1000;
        var RefractRay = hit(RefractDirection, ray.point + offset);
        if (RefractRay.collider)
        {
            Debug.DrawRay(ray.point,RefractDirection*RefractRay.distance,Color.black);
            //LineManager(ray.point, RefractDirection*RefractRay.distance + ray.point, lineCount, Color.black);
            lineCount++;
            return RefractRay;

        }
        else
        {
            Debug.DrawRay(ray.point, RefractDirection*100, Color.red);
            if (DrawVirtualRays) Debug.DrawRay(ray.point, -RefractDirection*100, Color.red);
            //LineManager(ray.point, RefractDirection*100 + ray.point, lineCount, Color.red);
            lineCount++;
            Vector2[] foo = { ray.point, RefractDirection + ray.point };
            rays.Add(foo);
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

    Vector2 GetIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        double m1 = (p2.y - p1.y) / (p2.x - p1.x);
        double m2 = (p4.y - p3.y) / (p4.x - p3.x);
        double xi = p3.y - p1.y + (m1 * p1.x) - (m2 * p3.x);
        xi /= m1 - m2;
        double yi = (xi - p1.x) * m1;
        yi += p1.y;
        return new Vector2((float)xi, (float)yi);
    }

    void AbberationCalculations()
    {
        for (int i = 0; i < rays.Count; i++)
        {
            double invslope = (rays[i][1].x - rays[i][0].x) / (rays[i][1].y - rays[i][0].y);
            double foo = yavg - rays[i][0].y;
            foo *= invslope;
            foo += rays[i][0].x;
            LSA.Add(foo);
            double slope = (rays[i][1].y - rays[i][0].y) / (rays[i][1].x - rays[i][0].x);
            foo = xavg - rays[i][0].x;
            foo *= slope;
            foo += rays[i][0].y;
            foo -= foo;
            TSA.Add(foo);
        }
    }
    void Update()
    {
        LSA = new List<double>();
        TSA = new List<double>();
        lineCount = 0;
        startAngle = (Mathf.PI/2) - (outputAngle / 2);
        for (int i = 0; i < RayCount; i++)
        {
            Vector2 direction = InitialCast(i);
            lineCount++;
            if (ray.collider)
            {
                int recursion = 1;
                Debug.DrawRay(pos,direction*ray.distance, Color.green);
                //LineManager(pos, ray.point, lineCount, Color.green);
                lineCount++;
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

        
        int count = 0;
        xavg = 0;
        yavg = 0;
        ydev = 0;
        xdev = 0;
        List<Vector2> intersects = new List<Vector2>();
        
        for (int i = 0; i < rays.Count; i++)
        {
            for (int j = 0; j < rays.Count; j++)
            {
                Vector2 vect2 = GetIntersect(rays[i][0], rays[i][1], rays[j][0], rays[j][1]);
                if (!(float.IsNaN(vect2.x) || float.IsNaN(vect2.y)))
                {
                    intersects.Add(vect2);
                    xavg += vect2.x;
                    yavg += vect2.y;
                    count++;
                }
            }
        }
        
        xavg /= count;
        yavg /= count;
        
        if (ParallelRays) focalPoint = yavg;
        
        for (int i = 0; i < intersects.Count; i++)
        {
            float diff = intersects[i].y - yavg;
            diff *= diff;
            ydev += diff;
            float xdiff = intersects[i].x - xavg;
            diff *= diff;
            xdev += diff;
        }
        
        ydev /= count;
        ydev = Mathf.Sqrt(ydev);
        AbberationCalculations();
        rays.Clear();
        _lineNames.Sort();
    }
}