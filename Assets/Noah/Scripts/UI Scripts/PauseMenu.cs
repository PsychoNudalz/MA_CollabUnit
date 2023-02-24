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
    [SerializeField] private GameObject pauseMenu;
    private bool optionsOpen = false;
    [SerializeField] private GameObject confirmBox;
    private bool confirmBoxOpen = false;

    public bool canBePaused = true;
    
    public void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && canBePaused)
        {
            MenuControl();
        }
        
    }

    private void MenuControl()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
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
            SwitchPauseMenuState();
        }
    }

    private void SwitchPauseMenuState()
    {
        if (!gameIsPaused)
        {
            pauseMenu.SetActive(true);
            PauseGame();

        }
        else
        {
            Resume();
        }
        
        // switch (gameIsPaused)
        // {
        //     case false:
        //         LeanTween.color(bg.GetComponent<RectTransform>(), bgOn, 0.2f).setIgnoreTimeScale(true);
        //         PauseGame();
        //         LeanTween.scale(pauseMenu, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutCubic)
        //             .setIgnoreTimeScale(true);
        //         break;
        //     case true:
        //         Resume();
        //         break;
        // }
    }

    private void PauseGame()
    {
        LeanTween.color(bg.GetComponent<RectTransform>(), bgOn, 0.2f).setIgnoreTimeScale(true);
        LeanTween.scale(pauseMenu, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true);
        gameIsPaused = true;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        LeanTween.color(bg.GetComponent<RectTransform>(), bgDefault, 0.2f);
        gameIsPaused = false;
        LeanTween.scale(pauseMenu, new Vector3(0, 0, 0), 0.2f).setEase(LeanTweenType.easeInOutCubic).setOnComplete(TurnOffPauseMenu);
    }

    private void TurnOffPauseMenu()
    {
        pauseMenu.SetActive(false);
    }

    private void TurnOffOptions()
    {
        optionsMenu.SetActive(false);
    }
    
    public void Options()
    {
        optionsOpen = true;
        optionsMenu.SetActive(true);
        LeanTween.scale(pauseMenu, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true).setOnComplete(TurnOffPauseMenu);
        LeanTween.scale(optionsMenu, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
    }
    
    public void CloseOptions()
    {
        optionsOpen = false;
        pauseMenu.SetActive(true);
        LeanTween.scale(pauseMenu, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
        LeanTween.scale(optionsMenu, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true).setOnComplete(TurnOffOptions);
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
        Time.timeScale = 1;
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        
    }
    
}
