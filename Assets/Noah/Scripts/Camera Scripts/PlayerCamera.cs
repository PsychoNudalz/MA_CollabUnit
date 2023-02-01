using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private CinemachineFreeLook cfl;
    private float xSpeedDefault, ySpeedDefault;
    
    // Fucking change this shit & make it work from the pause menu
    // Noah you're a fucking moron
    // Comment by Noah - 01/02/2023
    
    private bool gameIsPaused = false;
    void Start()
    {
        Cursor.visible = false;


        xSpeedDefault = cfl.m_XAxis.m_MaxSpeed;
        ySpeedDefault = cfl.m_YAxis.m_MaxSpeed;

    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Application.isEditor)
            {
                switch (Cursor.visible)
                {
                    case true:
                        Cursor.visible = false;
                        break;
                    
                    case false:
                        Cursor.visible = true;
                        break;
                }
            }

            switch (gameIsPaused)
            {
                case true:
                    gameIsPaused = false;
                    cfl.m_XAxis.m_MaxSpeed = xSpeedDefault;
                    cfl.m_YAxis.m_MaxSpeed = ySpeedDefault;
                    break;
                case false:
                    gameIsPaused = true;
                    cfl.m_YAxis.m_MaxSpeed = 0;
                    cfl.m_XAxis.m_MaxSpeed = 0;
                    break;
            }
            
            
        }
    }
}
