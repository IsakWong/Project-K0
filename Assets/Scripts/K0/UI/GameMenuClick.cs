using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class GameMenuClick : MonoBehaviour
{
    public String LoadingSceneName;

    private Animator _animator;

    // Use this for initialization
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _animator.SetBool("move_in", true);
            _animator.SetBool("move_out", false);
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<BlurOptimized>().enabled = true;
        }
    }

    public void OnResumeClick()
    {
        _animator.SetBool("move_in", false);
        _animator.SetBool("move_out", true);

        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<BlurOptimized>().enabled = false;
    }

    public void OnRestartClick()
    {
        SceneManager.LoadScene(LoadingSceneName);
    }

    public void OnBackToMenuClick()
    {
        SceneManager.LoadScene("MainScene");
    }
}