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