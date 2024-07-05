using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class MeshmakingMode : MonoBehaviour
{
    public GameObject meshmakingMode;
    
    private Toggle _meshmakingToggle;
    private bool wasOn = false;
    private MeshFilter _meshFilter;
    private EdgeCollider2D _edgeCollider2D;
    private Mesh _mesh;
    private Vector2[] _points;
    // Start is called before the first frame update
    void Start()
    {
        _meshmakingToggle = meshmakingMode.GetComponent<Toggle>();
        _meshFilter = GetComponent<MeshFilter>();
        _edgeCollider2D = GetComponent<EdgeCollider2D>();
        _points = _edgeCollider2D.points;
        _mesh = _meshFilter.mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if (_meshmakingToggle.isOn && !wasOn)
        {
            GetComponent<LensScript>().enabled = false;
            _mesh.Clear();
            wasOn = true;
        }
        else
        {
            wasOn = false;
        }
        

        
    }
}
