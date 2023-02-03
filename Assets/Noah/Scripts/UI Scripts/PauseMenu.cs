using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class PauseMenu : MonoBehaviour
{
    public bool gameIsPaused = false;
    [SerializeField] private GameObject bg;

    [SerializeField] private Color bgDefault, bgOn;
    [SerializeField] private GameObject optionsMenu;
    private bool optionsOpen = false;
    [SerializeField] private GameObject confirmBox;
    private bool confirmBoxOpen = false;
    
    public void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (optionsOpen)
            {
                CloseOptions();
            }
            else if (confirmBoxOpen)
            {
                CloseConfirmBox();
            }
            else
            {
                switch (gameIsPaused)
                {
                    case false:
                        LeanTween.color(bg.GetComponent<RectTransform>(), bgOn, 0.2f).setIgnoreTimeScale(true);
                        PauseGame();
                        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
                        break;
                    case true:
                        Resume();
                        break;
                }
            }
        }
    }

    public void PauseGame()
    {
        gameIsPaused = true;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        Time.timeScale = 1;
        LeanTween.color(bg.GetComponent<RectTransform>(), bgDefault, 0.2f);
        gameIsPaused = false;
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.2f).setEase(LeanTweenType.easeInOutCubic);
    }
    
    public void Options()
    {
        optionsOpen = true;
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
        LeanTween.scale(optionsMenu, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
    }
    
    public void CloseOptions()
    {
        optionsOpen = false;
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
        LeanTween.scale(optionsMenu, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
    }
    
    public void QuitGame()
    {
        confirmBoxOpen = true;
        LeanTween.scale(confirmBox, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
    }

    public void CloseConfirmBox()
    {
        confirmBoxOpen = false;
        LeanTween.scale(confirmBox, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
    }

    public void LoadMenu(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
    
}
