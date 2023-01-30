using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuButtons : MonoBehaviour
{
    
    // TO DO:
    // MERGE THIS SCRIPT WITH THE MENU MANAGER SO IT'S ALL TOGETHER
    // THIS IS A WASTE OF CODE TO PUT IT HERE
    
    
    [SerializeField] private float delayTimer;
    [SerializeField] private GameObject menuFrame;
    [SerializeField] private float menuLeaveTimer;
    
    [SerializeField] public GameObject[] menuButtons;
    [SerializeField] public GameObject optionsMenu;

    
    public void PlayGame(string sceneName)
    {
        if (sceneName == null)
        {
            Debug.Log("No scene name loaded");
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
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
        // Quick animation mockup
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic);
        yield return new WaitForSeconds(delayTimer);
        Application.Quit();

        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
    
}
