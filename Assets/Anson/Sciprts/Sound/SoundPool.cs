using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;


public class SoundPool : Sound
{
    [Header("Pool Handling")]
    [SerializeField]
    AudioSource[] sourcePool;

    [SerializeField]
    int sourcePoolSize = 6;

    [SerializeField]
    [Min(0)]
    int poolIndex = 0;

    private bool increasedIndex = false;

    private void Awake()
    {
        Initialise();
        sourcePool = new AudioSource[sourcePoolSize];
        AudioSource temp;
        string[] ignoreList = {"minVolume", "maxVolume", "rolloffFactor"};
        for (int i = 0; i < sourcePoolSize; i++)
        {
            temp = gameObject.AddComponent<AudioSource>();
            //temp = new AudioSource(source);

            DuplicateObjectScript.CopyPropertiesTo(source, temp, new List<string>(ignoreList));
            //source.clip = clip;
            sourcePool[i] = temp;
        }

        poolIndex = 0;
        source = sourcePool[poolIndex];

    }

    [ContextMenu("Play")]
    public override void Play()
    {
        increasedIndex = true;

        base.Play();
        poolIndex = (poolIndex + 1) % sourcePoolSize;
        source = sourcePool[poolIndex];

        increasedIndex = false;
    }

    [ContextMenu("PlayF")]
    public override void PlayF()
    {
        base.PlayF();
        if (!increasedIndex)
        {
            poolIndex = (poolIndex + 1) % sourcePoolSize;
        }
        source = sourcePool[poolIndex];

    }

    public override void Stop()
    {
        foreach (AudioSource a in sourcePool)
        {
            a.Stop();
        }

        //base.Stop();
    }

    public override void Pause()
    {
        foreach (AudioSource a in sourcePool)
        {
            a.Pause();
        }
    }

    public override void Resume()
    {
        foreach (AudioSource a in sourcePool)
        {
            a.UnPause();
        }
    }

    public void CopyAudioSource(AudioSource a, AudioSource b)
    {
        b.clip = a.clip;
        b.outputAudioMixerGroup = a.outputAudioMixerGroup;
        b.mute = a.mute;
        b.bypassEffects = a.bypassEffects;
        b.bypassListenerEffects = b.bypassListenerEffects;
    }
}