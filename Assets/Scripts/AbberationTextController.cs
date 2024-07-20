using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AbberationTextController : MonoBehaviour
{
    public TMP_Text text;
    public GameObject light;
    
    private RayCasting _rayCasting;
    // Start is called before the first frame update
    void Start()
    {
        _rayCasting = light.GetComponent<RayCasting>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        List<double> LSA = _rayCasting.LSA;
        List<double> TSA = _rayCasting.TSA;
        double LSAMax = LSA.Max();
        double TSAMax = TSA.Max();
        double LSAAvg = LSA.Average();
        double TSAAvg = TSA.Average();
        text.text = "LSA Max: " + LSAMax + "\nTSA Max: " + TSAMax + 
                    "\nLSA Average: " + LSAAvg + "\nTSA Average: " + TSAAvg;
    }
}
