using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayTutorialUI : MonoBehaviour
{
    // Start is called before the first frame update

    public RectTransform tut1, tut2, tut3, tutorialRect;

    public PauseMenu pm;

    private bool tutorial2open;

    public void StartTutorialUI()
    {
        tutorialRect.gameObject.SetActive(true);
        tutorial2open = true;
        pm.canBePaused = false;
        Cursor.visible = true;
        LeanTween.scale(tutorialRect, new Vector3(1, 1, 1), 0.3f)
            .setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);

        LeanTween.scale(tut1, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true).setDelay(.1f);

        LeanTween.scale(tut2, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true).setDelay(1.1f);

        LeanTween.scale(tut3, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true).setDelay(2.1f);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        LeanTween.scale(tutorialRect, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
        pm.canBePaused = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (tutorial2open)
        {
            if (GetComponent<TutorialMenu>().controllerConnected)
            {
                if (Gamepad.current.aButton.wasPressedThisFrame)
                {
                    StartGame();
                }
            }
        }
    }
}
