using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class KittenManager : MonoBehaviour
{
    public List<GameObject> kittenObjects = new List<GameObject>();
    [SerializeField] private GameObject text;

    private int i = 0;
    
    public void SpawnKitten()
    {
        if (i >= kittenObjects.Count)
            return;

        kittenObjects[i].SetActive(true);
        i++;
        StartCoroutine(KittenText());
    }

    private void Update()
    {
        if (Keyboard.current.commaKey.wasPressedThisFrame)
        {
            SpawnKitten();
        }
    }

    private IEnumerator KittenText()
    {
        text.SetActive(true);
        yield return new WaitForSeconds(4f);
        text.SetActive(false);
    }
}
