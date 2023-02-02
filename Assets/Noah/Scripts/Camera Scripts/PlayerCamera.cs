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
    [SerializeField] private PauseMenu pm;
    
    // Fucking change this shit & make it work from the pause menu
    // Noah you're a fucking moron
    // Comment by Noah - 01/02/2023
    
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

            switch (pm.gameIsPaused)
            {
                case true:
                    cfl.m_XAxis.m_MaxSpeed = xSpeedDefault;
                    cfl.m_YAxis.m_MaxSpeed = ySpeedDefault;
                    break;
                case false:
                    cfl.m_YAxis.m_MaxSpeed = 0;
                    cfl.m_XAxis.m_MaxSpeed = 0;
                    break;
            }
            
            
        }
    }
}
