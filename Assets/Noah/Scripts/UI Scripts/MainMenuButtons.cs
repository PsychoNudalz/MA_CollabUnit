using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class MainMenuButtons : MonoBehaviour
{
  
    [SerializeField] private float delayTimer;
    [SerializeField] public GameObject[] menuButtons;
    [SerializeField] public GameObject optionsMenu;
    private bool introMenuActive;
    [SerializeField] public GameObject introMenu, mainMenu, menuFrame;
    

    private bool canStartMenu;

    private void Start()
    {
        introMenu.SetActive(true);
        mainMenu.SetActive(false);

        canStartMenu = false;
        StartCoroutine(IntroStart());
    }

    private void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || ((Gamepad.current != null && Gamepad.current.rightStick.IsActuated())))
        {
            if(canStartMenu)
                ChangeMenu();
        }
    }

    private IEnumerator IntroStart()
    {
        yield return new WaitForSeconds(2f);
        canStartMenu = true;
        introMenu.SetActive(true);
    }

    private void ChangeMenu()
    {
        introMenu.SetActive(false);
        mainMenu.SetActive(true);

        LeanTween.scale(menuFrame, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInCubic);
        foreach (GameObject button in menuButtons)
        {
            LeanTween.scale(button, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInCubic);
        }
        
    }

    public void PlayGame(string sceneName)
    {
        StartCoroutine(LoadScene(sceneName));
    } public void PlayGame_I(int sceneIndex)
    {
        StartCoroutine(LoadScene(sceneIndex));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
        yield return new WaitForSeconds(delayTimer);
        SceneManager.LoadScene(sceneName);
    }    private IEnumerator LoadScene(int sceneName)
    {
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
        yield return new WaitForSeconds(delayTimer);
        SceneManager.LoadScene(sceneName);
    }

    public void Options()
    {
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
        LeanTween.scale(optionsMenu, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic);
    }

    public void CloseOptions()
    {
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic);
        LeanTween.scale(optionsMenu, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameDelay());
    }

    public void DisableButton()
    {
        foreach (GameObject button in menuButtons)
        {
            button.GetComponent<Button>().interactable = false;
        }
    }

    private IEnumerator QuitGameDelay()
    {
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
        yield return new WaitForSeconds(delayTimer);
        Application.Quit();

        // if (Application.isEditor)
        // {
        //     UnityEditor.EditorApplication.isPlaying = false;
        // }
    }
    
}
