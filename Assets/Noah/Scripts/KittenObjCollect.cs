using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KittenObjCollect : MonoBehaviour
{
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
        KittenManager.current.IncreaseKittenScore();
    }
}
