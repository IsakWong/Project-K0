using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public GameObject mask;

    public String NextStringName;
    public float LoadingMinTime = 2f;
    public AssetReference NextLevel;
    private AsyncOperationHandle<SceneInstance> async;
    private float _loadingTime = 0;

    // Use this for initialization
    public void BeginLoad()
    {
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        async = Addressables.LoadSceneAsync(NextLevel, activateOnLoad: false);

        yield return async;
    }

    bool activate = false;

    // Update is called once per frame
    void Update()
    {
        _loadingTime += Time.deltaTime;
        if (_loadingTime > LoadingMinTime)
        {
            mask.SetActive(true);
            if (async.IsValid() && async.Status == AsyncOperationStatus.Succeeded && !activate)
            {
                if (mask.GetComponent<Image>().color.a >= 0.9f)
                {
                    async.Result.ActivateAsync();
                    Debug.LogWarning("Loaded");
                    activate = true;
                }
            }
        }
    }
}