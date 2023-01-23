#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class DestructiveControllerWindow : EditorWindow
{
    Vector2 scrollPos;

    private BreakableObject[] controlledBreakableObject = Array.Empty<BreakableObject>();


    [MenuItem("Window/Destructive Controller")]
    public static void ShowWindow()
    {
        GetWindow<DestructiveControllerWindow>("Destructive Controller Window");
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("I have no idea what I am doing", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Active Breakable Objects"))
        {
            controlledBreakableObject = DestructiveController.GetActiveBreakableObjects();
        }

        if (GUILayout.Button("Refresh ALL Breakable Objects"))
        {
            controlledBreakableObject = DestructiveController.GetActiveBreakableObjects(true);
        }

        GUILayout.Label($"Number of Breakable Objects:{controlledBreakableObject.Length}", EditorStyles.boldLabel);


        if (GUILayout.Button("Initialise All Breakable Objects"))
        {
            DestructiveController.Initialise(controlledBreakableObject);
        }
        
        if (GUILayout.Button("Update All Breakable Objects"))
        {
            DestructiveController.UpdateObjects(controlledBreakableObject);
        }

        if (GUILayout.Button("Count All Breakable Parts"))
        {
            Debug.Log($"Number of Parts: {FindObjectsOfType<BreakablePart>().Length}");
        }


        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }


/*
    private void MarkDirty()
    {
        EditorUtility.SetDirty(cardHandler);
        EditorSceneManager.MarkSceneDirty(cardHandler.gameObject.scene);
        foreach (Card c in cardHandler.AllCards)
        {
            if (c)
            {
            EditorUtility.SetDirty(c.gameObject);
            }

        }
    }*/
}


public class DestructiveController
{
    public static BreakableObject[] GetActiveBreakableObjects(bool includeInactive = false)
    {
        // List<BreakableObject> temp = new List<BreakableObject>();
        // foreach (GameObject g in GameObject.FindObjectsOfType<BreakableObject>(false))
        // {
        //     temp.Add(g.GetComponent<BreakableObject>());
        // }
        BreakableObject[] temp = GameObject.FindObjectsOfType<BreakableObject>(includeInactive);
        Debug.Log($"Window hooked to {temp.Length} objects");
        return temp;
    }

    public static void UpdateObjects(BreakableObject[] breakableObjects)
    {
        foreach (BreakableObject breakableObject in breakableObjects)
        {
            try
            {
                breakableObject.UpdateValues();
                EditorUtility.SetDirty(breakableObject.gameObject);

                Debug.Log($"{breakableObject} initialise COMPLETE");
            }
            catch (Exception e)
            {
                Debug.LogError($"{breakableObject} initialise FAILEd");
                Debug.LogError(e);
            }
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

    }

    public static void Initialise(BreakableObject[] breakableObjects)
    {
        foreach (BreakableObject breakableObject in breakableObjects)
        {
            try
            {
                breakableObject.Initialise();
                EditorUtility.SetDirty(breakableObject.gameObject);

                Debug.Log($"{breakableObject} initialise COMPLETE");
            }
            catch (Exception e)
            {
                Debug.LogError($"{breakableObject} initialise FAILEd");
                Debug.LogError(e);
            }
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

    }
}

public class DestructiveWindowInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUI.changed)
        {
        }
    }
}
#endif