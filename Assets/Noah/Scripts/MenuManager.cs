using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MenuManager : MonoBehaviour
{
    
    // Script merged with MainMenuButtons.cs
    
    
    // private bool introMenuActive;
    // [SerializeField] public GameObject introMenu, mainMenu, gameLogo, titleScreenText;
    //
    // [SerializeField] public GameObject[] menuButtons;
    //
    // private bool canStartMenu;
    //
    // private void Start()
    // {
    //     introMenu.SetActive(true);
    //     mainMenu.SetActive(false);
    //     titleScreenText.SetActive(false);
    //
    //     canStartMenu = false;
    //     StartCoroutine(IntroStart());
    //
    //     LeanTween.scale(gameLogo, new Vector3(1, 1, 1), 1.5f).setEase(LeanTweenType.easeInBounce);
    // }
    //
    // private void Update()
    // {
    //     if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
    //     {
    //         if(canStartMenu)
    //             ChangeMenu();
    //     }
    // }
    //
    // private IEnumerator IntroStart()
    // {
    //     yield return new WaitForSeconds(2f);
    //     canStartMenu = true;
    //     titleScreenText.SetActive(true);
    // }
    //
    // private void ChangeMenu()
    // {
    //     introMenu.SetActive(false);
    //     mainMenu.SetActive(true);
    //
    //     foreach (GameObject button in menuButtons)
    //     {
    //         LeanTween.scale(button, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInCubic);
    //     }
    //     
    // }
    
    
}
