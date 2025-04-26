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
    }

    public void OnResumeClick()
    {
        GetComponent<UIAnimFadeMove>().Hide();
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