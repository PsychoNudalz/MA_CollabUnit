using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class PauseMenu : MonoBehaviour
{
    private bool gameIsPaused = false;
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
                Resume();
            }
            else
            {
                switch (gameIsPaused)
                {
                    case false:
                        LeanTween.color(bg.GetComponent<RectTransform>(), bgOn, 0.2f);
                        gameIsPaused = true;
                        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutCubic);
                        break;
                    case true:
                        Resume();
                        break;
                }
            }
        }
    }

    public void Resume()
    {
        LeanTween.color(bg.GetComponent<RectTransform>(), bgDefault, 0.2f);
        gameIsPaused = false;
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.2f).setEase(LeanTweenType.easeInOutCubic);
    }
    
    public void Options()
    {
        optionsOpen = true;
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
        LeanTween.scale(optionsMenu, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic);
    }
    
    public void CloseOptions()
    {
        optionsOpen = false;
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic);
        LeanTween.scale(optionsMenu, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
    }
    
    public void QuitGame()
    {
        confirmBoxOpen = true;
        LeanTween.scale(confirmBox, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic);
    }

    public void CloseConfirmBox()
    {
        confirmBoxOpen = false;
        LeanTween.scale(confirmBox, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
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
