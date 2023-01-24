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

    private BreakableStructure[] controlledbreakableStructure = Array.Empty<BreakableStructure>();


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
            controlledbreakableStructure = DestructiveController.GetActivebreakableStructures();
        }

        if (GUILayout.Button("Refresh ALL Breakable Objects"))
        {
            controlledbreakableStructure = DestructiveController.GetActivebreakableStructures(true);
        }

        GUILayout.Label($"Number of Breakable Objects:{controlledbreakableStructure.Length}", EditorStyles.boldLabel);


        if (GUILayout.Button("Initialise All Breakable Objects"))
        {
            DestructiveController.Initialise(controlledbreakableStructure);
        }
        
        if (GUILayout.Button("Update All Breakable Objects"))
        {
            DestructiveController.UpdateObjects(controlledbreakableStructure);
        }
        
        if (GUILayout.Button("Reset Connection All Breakable Objects"))
        {
            DestructiveController.ResetConnections(controlledbreakableStructure);
        }
        
        GUILayout.Space(15f);

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
    public static BreakableStructure[] GetActivebreakableStructures(bool includeInactive = false)
    {
        // List<breakableStructure> temp = new List<breakableStructure>();
        // foreach (GameObject g in GameObject.FindObjectsOfType<breakableStructure>(false))
        // {
        //     temp.Add(g.GetComponent<breakableStructure>());
        // }
        BreakableStructure[] temp = GameObject.FindObjectsOfType<BreakableStructure>(includeInactive);
        Debug.Log($"Window hooked to {temp.Length} objects");
        return temp;
    }

    public static void UpdateObjects(BreakableStructure[] breakableStructures)
    {
        foreach (BreakableStructure breakableStructure in breakableStructures)
        {
            try
            {
                breakableStructure.UpdateValues();
                EditorUtility.SetDirty(breakableStructure.gameObject);
                MarkBreakablePartDirty(breakableStructure.BreakableComponents);

                Debug.Log($"{breakableStructure} update COMPLETE");
            }
            catch (Exception e)
            {
                Debug.LogError($"{breakableStructure} update FAILED");
                Debug.LogError(e);
            }
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

    }

    public static void Initialise(BreakableStructure[] breakableStructures)
    {
        foreach (BreakableStructure breakableStructure in breakableStructures)
        {
            try
            {
                breakableStructure.Initialise();
                EditorUtility.SetDirty(breakableStructure.gameObject);
                MarkBreakablePartDirty(breakableStructure.BreakableComponents);
                Debug.Log($"{breakableStructure} initialise COMPLETE");
            }
            catch (Exception e)
            {
                Debug.LogError($"{breakableStructure} initialise FAILED");
                Debug.LogError(e);
            }
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

    }

    public static void ResetConnections(BreakableStructure[] breakableStructures)
    {
        foreach (BreakableStructure breakableStructure in breakableStructures)
        {
            try
            {
                breakableStructure.ResetConnections();
                EditorUtility.SetDirty(breakableStructure.gameObject);
                PrefabUtility.RecordPrefabInstancePropertyModifications(breakableStructure.gameObject);

                MarkBreakablePartDirty(breakableStructure.BreakableComponents);
                Debug.Log($"{breakableStructure} initialise COMPLETE");
            }
            catch (Exception e)
            {
                Debug.LogError($"{breakableStructure} initialise FAILED");
                Debug.LogError(e);
            }
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    public static void MarkBreakablePartDirty(BreakableComponent[] breakableComponents)
    {
        foreach (BreakableComponent breakableComponent in breakableComponents)
        {
            EditorUtility.SetDirty(breakableComponent.gameObject);
            EditorUtility.SetDirty(breakableComponent);
            // PrefabUtility.RecordPrefabInstancePropertyModifications(breakableComponent.gameObject);
            // PrefabUtility.RecordPrefabInstancePropertyModifications(breakableComponent);
        }
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