using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SoundAbstract : MonoBehaviour
{
    protected SoundManager soundManager;
    public SoundManager SoundManager
    {
        get => soundManager;
        set => soundManager = value;
    }
    
    public abstract void Play();

    public abstract void PlayF();

    public abstract void Stop();
    
    public abstract bool IsPlaying();

    public abstract void Pause();

    public abstract void Resume();

    protected virtual void Initialise()
    {
    }

    public virtual AudioClip GetClip()
    {
        return null;
    }

    public virtual void SetClip(AudioClip audioClip)
    {
        
    }

    protected virtual void StartBehaviour()
    {
        
    }

    private void Awake()
    {
        Initialise();
    }

    private void Start()
    {
        StartBehaviour();
    }

}
