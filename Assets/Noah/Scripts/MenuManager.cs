using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class MenuManager : MonoBehaviour
{
    // hello Anson
    // if you are reading this
    
    // understand that this was created by me when i was tired and rushing
    // the code is shit. I KNOW IT IS 😤

    private bool introMenuActive;
    [SerializeField] public GameObject introMenu, mainMenu;

    private float timeToNewMenuCurrent;
    private float timeToNewMenuMax = 5f;

    private bool timerActive = true;
    
    private void Start()
    {
        introMenuActive = true;
        timerActive = true;
        
        introMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    private void Update()
    {
        if (timerActive)
        {
            timeToNewMenuCurrent += Time.deltaTime;
        }

        if (timeToNewMenuCurrent >= timeToNewMenuMax)
        {
            ChangeMenu();
            timerActive = false;
        }
    }

    private void ChangeMenu()
    {
        introMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    
    
}
