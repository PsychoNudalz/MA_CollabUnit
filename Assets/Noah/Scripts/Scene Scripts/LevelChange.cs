using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        yield return null;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(2);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
           
            if (asyncOperation.progress >= 0.9f)
            {
                if (Keyboard.current.f4Key.wasPressedThisFrame)
                {
                    asyncOperation.allowSceneActivation = true;
                }
                if (Gamepad.current != null)
                {
                    if (Gamepad.current.selectButton.wasPressedThisFrame)
                    {
                        asyncOperation.allowSceneActivation = true;
                    }
                }
            }

            yield return null;
        }
    }
    
}
