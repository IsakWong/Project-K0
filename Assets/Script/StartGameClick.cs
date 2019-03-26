using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class StartGameClick : MonoBehaviour
{
    // Use this for initialization
    public AssetReference Level1;
    public AssetReference Level2;
    public AssetReference Level3;

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

    public void OnLevel1Click()
    {
        Addressables.LoadSceneAsync(Level1);
    }

    public void OnLevel2Click()
    {
        Addressables.LoadSceneAsync(Level2);
    }

    public void OnLevel3Click()
    {
        Addressables.LoadSceneAsync(Level3);
    }
}