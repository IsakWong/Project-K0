using System;
using K1.UI;
using UnityEngine;

public class K0UILevelMain : UIPanel
{
    public GameObject Menu;

    protected void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape) && !Menu.activeInHierarchy)
        {
            Menu.GetComponent<UIAnimFadeMove>().Show(1.0f);
        }
    }
}