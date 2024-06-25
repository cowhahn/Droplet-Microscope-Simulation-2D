using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaballController : MonoBehaviour
{
    public int resolution;
    public GameObject camera;
    public float threshold;
    
    private GameObject[] _metaballs;
    private float[,] _fieldStrength;
    private Vector2 _startLoc;
    private float _dy, _dx;
    private Mesh _mesh;
    private Vector3 _vertices;
    private int[] _triangles;
    private EdgeCollider2D _edgeCollider2D;
    
    // Start is called before the first frame update
    void Start()
    {
        
        _metaballs = GameObject.FindGameObjectsWithTag("Metaball");
        _fieldStrength = new float[resolution,resolution];
        Rect rect = camera.GetComponent<Camera>().rect;
        float height = rect.height / 2;
        float width = rect.width / 2;
        _startLoc = new Vector2(camera.transform.position.x - width, camera.transform.position.y + height);
        _dy = height * 2 / resolution;
        _dx = width * 2 / resolution;
    }

    float FieldFunction(Vector2 pos, Vector2 tran)
    {
        float foo = tran.x - pos.x;
        foo *= foo;
        float bar = tran.y - pos.y;
        bar *= bar;
        return 1 / Mathf.Sqrt(foo + bar);
    }
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                foreach (GameObject metaball in _metaballs)
                {
                    _fieldStrength[i,j]  += FieldFunction(new Vector2((j*_dx)+_startLoc.x, (i*_dy)+_startLoc.y),
                        metaball.transform.position);
                }
                if (_fieldStrength[i, j] < threshold) _fieldStrength[i, j] = 0;
                else 
            }
        } 
    }
}
