using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    public AssetReference Location;
    void Start()
    {
        StartCoroutine("Init");
    }

    IEnumerator Init()
    {
        yield return Addressables.InitializeAsync();
        yield return Addressables.LoadSceneAsync(Location);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
