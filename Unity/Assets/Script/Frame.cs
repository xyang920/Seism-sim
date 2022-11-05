using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame : MonoBehaviour
{
    [Header("Render Game Objects")]
    public GameObject TopFlange;

    public GameObject Web;
    public GameObject BotFlange;

    [Header("Frame Section Attributes")]
    public float TotatDepth;

    public float FlangeWidth;
    public float FlangeThickness;
    public float WebThickness;

    // Start is called before the first frame update
    private void Start()
    {
        UpdateSection(TotatDepth, FlangeWidth, FlangeThickness, WebThickness);
    }

    public void UpdateSection(float totatDepth, float flangeWidth, float flangeThickness, float webThickness)
    {
        //reset values
        TotatDepth = totatDepth;
        FlangeWidth = flangeWidth;
        FlangeThickness = flangeThickness;
        WebThickness = webThickness;
    }
}