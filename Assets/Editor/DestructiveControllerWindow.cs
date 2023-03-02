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

    private BreakableStructureController[] controlledbreakableStructure = Array.Empty<BreakableStructureController>();


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

        if (GUILayout.Button("Refresh Active Breakables"))
        {
            controlledbreakableStructure = DestructiveController.GetActivebreakableStructures();
        }
        if (GUILayout.Button("Refresh Active IgnoreParent Breakables"))
        {
            controlledbreakableStructure = DestructiveController.GetActivebreakableStructures(false,true
            );
        }

        if (GUILayout.Button("Refresh ALL Breakables"))
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
        if (GUILayout.Button("Show Shells"))
        {
            DestructiveController.ShowCollectiveFractures(controlledbreakableStructure);
        }

        if (GUILayout.Button("Hide Shells"))
        {
            DestructiveController.HideCollectiveFractures(controlledbreakableStructure);
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
    public static BreakableStructureController[] GetActivebreakableStructures(bool includeInactive = false, bool ignoreParent = false)
    {
        // List<breakableStructure> temp = new List<breakableStructure>();
        // foreach (GameObject g in GameObject.FindObjectsOfType<breakableStructure>(false))
        // {
        //     temp.Add(g.GetComponent<breakableStructure>());
        // }
        BreakableStructureController[] temp =
            GameObject.FindObjectsOfType<BreakableStructureController>(includeInactive);

        if (ignoreParent)
        {
            List<BreakableStructureController> temp2 = new List<BreakableStructureController>();

            foreach (BreakableStructureController b in temp)
            {
                if (b.IgnoreParent)
                {
                    temp2.Add(b);
                }
            }

            temp = temp2.ToArray();
        }
        Debug.Log($"Window hooked to {temp.Length} objects");
        return temp;
    }

    public static void UpdateObjects(BreakableStructureController[] breakableStructures)
    {
        ShowCollectiveFractures(breakableStructures);

        foreach (BreakableStructureController breakableStructure in breakableStructures)
        {
            try
            {
                breakableStructure.UpdateValues();
                EditorUtility.SetDirty(breakableStructure.gameObject);
                MarkBreakablePartDirty(breakableStructure.AllBreakableComponents);

                Debug.Log($"{breakableStructure} update COMPLETE");
            }
            catch (Exception e)
            {
                Debug.LogError($"{breakableStructure} update FAILED");
                Debug.LogError(e);
            }
        }
        HideCollectiveFractures(breakableStructures);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    public static void Initialise(BreakableStructureController[] breakableStructures)
    {
        ShowCollectiveFractures(breakableStructures);
        foreach (BreakableStructureController breakableStructure in breakableStructures)
        {
            try
            {
                breakableStructure.Initialise();
                EditorUtility.SetDirty(breakableStructure.gameObject);
                MarkBreakablePartDirty(breakableStructure.AllBreakableComponents);
                Debug.Log($"{breakableStructure} initialise COMPLETE");
            }
            catch (Exception e)
            {
                Debug.LogError($"{breakableStructure} initialise FAILED");
                Debug.LogError(e);
            }
        }
        HideCollectiveFractures(breakableStructures);


        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    public static void ResetConnections(BreakableStructureController[] breakableStructures)
    {
        foreach (BreakableStructureController breakableStructure in breakableStructures)
        {
            try
            {
                breakableStructure.ResetConnections();
                EditorUtility.SetDirty(breakableStructure.gameObject);
                PrefabUtility.RecordPrefabInstancePropertyModifications(breakableStructure.gameObject);

                MarkBreakablePartDirty(breakableStructure.AllBreakableComponents);
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

    public static void ShowCollectiveFractures(BreakableStructureController[] breakableStructures)
    {
        foreach (BreakableStructureController structure in breakableStructures)
        {
            foreach (BreakableComponent breakableComponent in structure.BreakableCollectives)
            {
                if (breakableComponent is BreakableCollective bc)
                {
                    bc.FlipShell(true);
                }
            }
        }
        
    }

    public static void HideCollectiveFractures(BreakableStructureController[] breakableStructures)
    {
        foreach (BreakableStructureController structure in breakableStructures)
        {
            foreach (BreakableComponent breakableComponent in structure.BreakableCollectives)
            {
                if (breakableComponent is BreakableCollective bc)
                {
                    bc.FlipShell(false);
                }
            }
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