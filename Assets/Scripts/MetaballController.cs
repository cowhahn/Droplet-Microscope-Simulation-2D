using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MetaballController : MonoBehaviour
{
    public int resolution;
    public float threshold;
    public GameObject camera;
    
    
    private GameObject[] _metaballs;
    private float[,] _fieldStrength;
    private Vector2 _startLoc;
    private float _dy, _dx;
    private Mesh _mesh;
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private EdgeCollider2D _edgeCollider2D;
    private Vector3[] _lastPosition;
    float FieldFunction(Vector2 pos, Vector2 tran)
    {
        float foo = pos.x - tran.x;
        foo *= foo;
        float bar = pos.y - tran.y;
        bar *= bar;
        return 1 / (foo + bar);
    }

    void CreateMetaballField()
    {
        _fieldStrength = new float[resolution, resolution];
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                foreach (GameObject metaball in _metaballs)
                {
                    _fieldStrength[i, j] += FieldFunction(
                        new Vector2((j * _dx) + _startLoc.x, (i * _dy) + _startLoc.y),
                        metaball.transform.position);
                    //Debug.Log(_fieldStrength[i,j]);
                }
                
                if (_fieldStrength[i, j] > threshold) _fieldStrength[i, j] = 1;
                else _fieldStrength[i, j] = 0;
            }
        }
    }

    int[,] ConnectVerts(int[,] boarderedField)
    {
        int[,] msIdents = new int[resolution + 1, resolution + 1];
        for (int i = 1; i < resolution + 2; i++)
        {
            for (int j = 1; j < resolution + 2; j++)
            {
                int ident = 0b0000;
                if (boarderedField[i, j] == 1) ident += 0b0010;
                if (boarderedField[i - 1, j] == 1) ident += 0b0001;
                if (boarderedField[i - 1, j - 1] == 1) ident += 0b1000;
                if (boarderedField[i, j - 1] == 1) ident += 0b0100;
                i -= 1;
                j -= 1;
                switch (ident)                      
                {
                    case 0b0000:
                        msIdents[i, j] = 0b0000;
                        break;
                    case 0b0100:
                        msIdents[i, j] = 0b1100;
                        break;
                    case 0b0010:
                        msIdents[i, j] = 0b0110;
                        break;
                    case 0b0110:
                        msIdents[i, j] = 0b1010;
                        break;
                    case 0b0001:
                        msIdents[i, j] = 0b0011;
                        break;
                    case 0b0101:
                        msIdents[i, j] = 0b1111;
                        break;
                    case 0b0011:
                        msIdents[i, j] = 0b0101;
                        break;
                    case 0b0111:
                        msIdents[i, j] = 0b1001;
                        break;
                    case 0b1000:
                        msIdents[i, j] = 0b1001;
                        break;
                    case 0b1100:
                        msIdents[i, j] = 0b0101;
                        break;
                    case 0b1010:
                        msIdents[i, j] = 0b11111;
                        break;
                    case 0b1110:
                        msIdents[i, j] = 0b0011;
                        break;
                    case 0b1001:
                        msIdents[i, j] = 0b1010;
                        break;
                    case 0b1101:
                        msIdents[i, j] = 0b0110;
                        break;
                    case 0b1011:
                        msIdents[i, j] = 0b1100;
                        break;
                    case 0b1111:
                        msIdents[i, j] = 0b10000;
                        break;
                }
                i += 1;
                j += 1;
            }
        }

        return msIdents;
    }
    void MarchingSquares()
    {
        int[,] boarderedField = new int[resolution + 2, resolution + 2];
        List<Vector3> verts = new List<Vector3>();

        for (int i = 0; i < resolution + 2; i++)
        {
            if (i > 0 && i < resolution)
            {
                for (int j = 0; j < resolution; j++)
                {
                    boarderedField[i, j + 1] = (int)_fieldStrength[i - 1, j];
                }
            }

            boarderedField[0, i] = 0;
            boarderedField[i, 0] = 0;
            boarderedField[resolution + 1, i] = 0;
            boarderedField[i, resolution + 1] = 0;
        }
        int[,] vertConnects = ConnectVerts(boarderedField);
        _vertices = new List<Vector3>();
        _vertices.Clear();
        _triangles = new List<int>();
        _triangles.Clear();
        List<Vector2> colliderPoints = new List<Vector2>();
        for (int i = 0; i < resolution + 1; i++)
        {
            for (int j = 0; j < resolution + 1; j++)
            {
                switch (vertConnects[i, j])
                { 
                        
                    case 0b10000:
                        _vertices.Add(new Vector3(j * _dx + _startLoc.x, i * _dy + _startLoc.y, 0));
                        _vertices.Add(new Vector3((j + 1) * _dx + _startLoc.x, i * _dy + _startLoc.y, 0));
                        _vertices.Add(new Vector3(j * _dx + _startLoc.x, (i - 1) * _dy + _startLoc.y, 0));
                        _vertices.Add(new Vector3((j + 1) * _dx + _startLoc.x, (i - 1) * _dy + _startLoc.y, 0));
                        _triangles.Add(_vertices.Count - 4);
                        _triangles.Add(_vertices.Count - 2);
                        _triangles.Add(_vertices.Count - 3);
                        _triangles.Add(_vertices.Count - 2);
                        _triangles.Add(_vertices.Count - 1);
                        _triangles.Add(_vertices.Count - 3);
                        break;
                        case 0b1100:
                            colliderPoints.Add(new Vector2((j + 1) * _dx + _startLoc.x, i * _dy + _startLoc.y));
                            colliderPoints.Add(new Vector2(j * _dx + _startLoc.x, (i - 1) * _dy + _startLoc.y));
                            break;
                        case 0b0110:
                            colliderPoints.Add(new Vector2(j * _dx + _startLoc.x, i * _dy + _startLoc.y));
                            colliderPoints.Add(new Vector2((j + 1) * _dx + _startLoc.x, (i - 1) * _dy + _startLoc.y));
                            break;
                        case 0b0011:
                            colliderPoints.Add(new Vector2((j + 1) * _dx + _startLoc.x, i * _dy + _startLoc.y )); 
                            colliderPoints.Add(new Vector2(j * _dx + _startLoc.x, (i - 1) * _dy + _startLoc.y));
                            break;
                        case 0b1001:
                            colliderPoints.Add(new Vector2(j * _dx + _startLoc.x, i * _dy + _startLoc.y));
                            colliderPoints.Add(new Vector2((j + 1) * _dx + _startLoc.x, (i - 1) * _dy + _startLoc.y));
                            break;
                        
                }
            }
        }
        _edgeCollider2D.points = colliderPoints.ToArray();
    }
    void Start()
    {

        _mesh = GetComponent<MeshFilter>().mesh;
        _edgeCollider2D = GetComponent<EdgeCollider2D>();
        
        _metaballs = GameObject.FindGameObjectsWithTag("Metaball");
        _fieldStrength = new float[resolution,resolution];
        Rect rect = camera.GetComponent<Camera>().rect;
        float height = 5;
        float width = 9;
        //Debug.Log(width);
        _startLoc = new Vector2(camera.transform.position.x - width, camera.transform.position.y - height);
        _dy = (height * 2) / resolution;
        _dx = (width * 2) / resolution;
        _lastPosition = new Vector3[_metaballs.Length];
    }
    
    void Update()
    {
        bool hasMoved = false;
        for (int i = 0; i < _metaballs.Length; i++)
        {
            if (_metaballs[i].transform.position != _lastPosition[i])
            {
                hasMoved = true;
                _lastPosition[i] = _metaballs[i].transform.position;
                break;
            }
        }
        if (hasMoved)
        {
            CreateMetaballField();
            MarchingSquares();
            _mesh.vertices = _vertices.ToArray();
            _mesh.triangles = _triangles.ToArray();
            _mesh.RecalculateNormals();
        }
        
        
    }
}
