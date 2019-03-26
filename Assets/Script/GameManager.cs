using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMnager : MonoBehaviour
{
    private int Tree_sum;
    private int Tree_now;


    private static GameMnager _cachedCurrent;

    public static GameMnager Current
    {
        get { return _cachedCurrent; }
    }


    // Use this for initialization
    void Start()
    {
        Tree_now = 0;
        _cachedCurrent = this;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddTree()
    {
        Tree_now++;
    }
}