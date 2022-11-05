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
    public float TotalDepth;

    public float FlangeWidth;
    public float FlangeThickness;
    public float WebThickness;

    // Start is called before the first frame update
    private void Start()
    {
        UpdateSection(TotalDepth, FlangeWidth, FlangeThickness, WebThickness);
    }

    public void UpdateSection(float totalDepth, float flangeWidth, float flangeThickness, float webThickness)
    {
        //reset values
        TotalDepth = totalDepth;
        FlangeWidth = flangeWidth;
        FlangeThickness = flangeThickness;
        WebThickness = webThickness;

        //update overall scale
        var scale = transform.localScale;
        scale.y = TotalDepth;
        scale.x = FlangeWidth;
        transform.localScale = scale;

        //update web scale
        Web.transform.parent = null;
        var webScale = Web.transform.localScale;
        webScale.x = webThickness;
        Web.transform.localScale = webScale;
        Web.transform.parent = this.transform;

        //update flange scale
        TopFlange.transform.parent = null;
        BotFlange.transform.parent = null;
        var flangeScale = TopFlange.transform.localScale;
        flangeScale.y = flangeThickness;
        TopFlange.transform.localScale = flangeScale;
        BotFlange.transform.localScale = flangeScale;
        TopFlange.transform.parent = this.transform;
        BotFlange.transform.parent = this.transform;

        //update flange placement
        var topPos = TopFlange.transform.localPosition;
        var botPos = BotFlange.transform.localPosition;
        var vertAdjust = flangeThickness / totalDepth / 2.00f;
        Debug.Log(vertAdjust);
        topPos.y = 0.5f - vertAdjust;
        botPos.y = -0.5f + vertAdjust;
        TopFlange.transform.localPosition = topPos; Debug.Log(topPos);
        BotFlange.transform.localPosition = botPos; Debug.Log(botPos);
    }
}