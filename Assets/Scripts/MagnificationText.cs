using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MagnificationText : MonoBehaviour
{
    public TMP_Text text;
    public GameObject light;

    private RayCasting _rayCasting;
    
    void Start()
    {
        _rayCasting = light.GetComponent<RayCasting>();
    }
    
    void Update()
    {
        if (!_rayCasting.ParallelRays)
        {
            float magnification = _rayCasting.yavg / light.transform.position.y;
            magnification = Mathf.Abs(magnification);
            text.text = "Magnification: " + magnification;
        }
    }
}
