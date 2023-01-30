using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButtons : MonoBehaviour
{

    [SerializeField] private float delayTimer;


    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private GameObject menuFrame;
    [SerializeField] private float menuLeaveTimer;
    
    public void PlayGame()
    {
        
    }

    public void Options()
    {
        
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameDelay());
    }

    private IEnumerator QuitGameDelay()
    {
        // Quick animation mockup
        LeanTween.move(menuFrame, new Vector3(-10, -500, 0), menuLeaveTimer).setEase(LeanTweenType.easeInOutCubic);
        yield return new WaitForSeconds(delayTimer);
        Application.Quit();

        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
    
}
