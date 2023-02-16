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
    
    [ContextMenu("Play")]
    public abstract void Play();

    [ContextMenu("PlayF")]
    public abstract void PlayF();

    [ContextMenu("Stop")]
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
