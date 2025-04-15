using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterUI : MonoBehaviour
{
    public static WaterUI WaterDisplay;

    // Use this for initialization
    void Start()
    {
        WaterDisplay = this;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetWaterFloat(float _value)
    {
        GetComponent<Text>().text = ((int)_value).ToString();
    }
}