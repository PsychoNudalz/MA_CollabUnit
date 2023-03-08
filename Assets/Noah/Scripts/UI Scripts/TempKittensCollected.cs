using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TempKittensCollected : MonoBehaviour
{

    [SerializeField] private RectTransform kittenCollectedUI;
    [SerializeField] private PauseMenu pm;
    [SerializeField] private bool isLevel1 = false;

    
    
    private void Start()
    {
        pm = GetComponent<PauseMenu>();
        kittenCollectedUI.gameObject.SetActive(false);
        LeanTween.scale(kittenCollectedUI, new Vector3(0, 0, 0), 0).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.commaKey.wasPressedThisFrame)
        {
            if(isLevel1)
                ContinueGame();
        }
    }

    private void ContinueGame()
    {
        pm.canBePaused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        kittenCollectedUI.gameObject.SetActive(true);
        LeanTween.scale(kittenCollectedUI, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true);
    }

    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pm.canBePaused = true;
    }
    
}
