using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatPlayerController : MonoBehaviour
{
    public static CatPlayerController current;
    // Start is called before the first frame update
    void Awake()
    {
        current = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
