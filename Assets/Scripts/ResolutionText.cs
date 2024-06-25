using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionText : MonoBehaviour
{

    public TMP_Text text;
    public GameObject light;
    private RayCasting _scriptValues;
    void Start()
    {
        _scriptValues = light.GetComponent<RayCasting>();
    }

    // Update is called once per frame
    void Update()
    {
        float rootSqrMean = Mathf.Sqrt((_scriptValues.xdev * _scriptValues.xdev) + (_scriptValues.ydev * _scriptValues.ydev));
        text.text = "Resolution: " + _scriptValues.xdev + ", " + _scriptValues.ydev + ", " + rootSqrMean;
    }
}
