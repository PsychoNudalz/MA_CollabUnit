using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class KittenObjCollect : MonoBehaviour
{
    private float kittenScore;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCat"))
        {
            IncreaseKittenScore();
        }
        
    }

    private void IncreaseKittenScore()
    {
        gameObject.SetActive(false);
        kittenScore++;
        Debug.Log("Current kitten score: " + kittenScore);
    }
    
}
