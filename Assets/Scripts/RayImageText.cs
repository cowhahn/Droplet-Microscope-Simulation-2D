using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class RayImageText : MonoBehaviour
{
    public TMP_Text text;
    public GameObject lightSource;
    private RayCasting _scriptValues;
    void Start()
    {
        _scriptValues = lightSource.GetComponent<RayCasting>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_scriptValues.ParallelRays)
        {
            text.text = "Focal Point at: " + _scriptValues.yavg;
        }
        else
        {
            text.text = "Image at: " + _scriptValues.yavg;
        }
    }
}
