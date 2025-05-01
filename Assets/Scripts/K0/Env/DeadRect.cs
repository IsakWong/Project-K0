using System.Collections;
using System.Collections.Generic;
using K0;
using UnityEngine;

public class DeadRect : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SlimeCharacter>() != null)
        {
            var gameMode = KGameCore.Instance.CurrentGameMode as K0GameMode;
            gameMode.Lose();

        }
    }
}