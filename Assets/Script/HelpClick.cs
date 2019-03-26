using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpClick : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnBackClick()
    {
        var mainMenu = transform.parent.GetChild(0).gameObject.GetComponent<MainMenuClick>();
        mainMenu._target = mainMenu.TargetCameras[0];
        mainMenu.gameObject.GetComponent<Animator>().SetBool("move_in", true);
        mainMenu.gameObject.GetComponent<Animator>().SetBool("move_out", false);
        GetComponent<Animator>().SetBool("move_out", true);
        GetComponent<Animator>().SetBool("move_in", false);
    }
}