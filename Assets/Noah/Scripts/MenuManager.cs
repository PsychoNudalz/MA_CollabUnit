using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MenuManager : MonoBehaviour
{
    // hello Anson
    // if you are reading this
    
    // understand that this was created by me when i was tired and rushing
    // the code is shit. I KNOW IT IS 😤

    private bool introMenuActive;
    [SerializeField] public GameObject introMenu, mainMenu;

    private void Start()
    {
        introMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            ChangeMenu();
        }
    }

    private void ChangeMenu()
    {
        introMenu.SetActive(false);
        mainMenu.SetActive(true);
        LeanTween.scale(mainMenu, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInBounce);
    }
    
    
}
