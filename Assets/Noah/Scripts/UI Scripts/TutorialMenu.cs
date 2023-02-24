using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TutorialMenu : MonoBehaviour
{
    private bool controllerConnected;
    [SerializeField] private GameObject tutorialMenuKeyboard, tutorialMenuController;

    private EventSystem eventSystem;
    
    [SerializeField] private PauseMenu pm;
    
    // Start is called before the first frame update
    void Start()
    {
        eventSystem = EventSystem.current;
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    private IEnumerator StartGame()
    {
        yield return new WaitForSecondsRealtime(.5f);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;  
        pm.canBePaused = false;

        if (Gamepad.all.Count > 0)
        {
            controllerConnected = true;
        }
        
        OpenTutorial();
    }

    private void OpenTutorial()
    {
        if (controllerConnected)
        {
            tutorialMenuController.SetActive(true);
            LeanTween.scale(tutorialMenuController, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutCubic)
                .setIgnoreTimeScale(true);
            eventSystem.SetSelectedGameObject(tutorialMenuController);
        }
        else
        {
            tutorialMenuKeyboard.SetActive(true);
            LeanTween.scale(tutorialMenuKeyboard, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutCubic)
                .setIgnoreTimeScale(true);
        }
    }

    public void CloseTutorial()
    {
        if (controllerConnected)
        {
            LeanTween.scale(tutorialMenuController, new Vector3(0, 0, 0), 0.2f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
            tutorialMenuController.SetActive(false);
        }
        else
        {
            LeanTween.scale(tutorialMenuKeyboard, new Vector3(0, 0, 0), 0.2f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
            tutorialMenuKeyboard.SetActive(false);
        }

        Time.timeScale = 1f;
        pm.canBePaused = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
}
