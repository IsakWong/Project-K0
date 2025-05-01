using System;
using System.Collections;
using System.Collections.Generic;
using K0;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class WinRect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    public AssetReference NextLevel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) ;
        {
            var gameMode = KGameCore.Instance.CurrentGameMode as K0GameMode;
            gameMode.Win(NextLevel);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
