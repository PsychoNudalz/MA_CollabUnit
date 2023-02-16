using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

/// <summary>
/// plays a sound
/// </summary>
[RequireComponent(typeof(AudioSource))]
[System.Serializable]
public class Sound : SoundAbstract
{
    [SerializeField]
    protected AudioSource source;

    public AudioSource Source => source;

    [Range(0f, 3f)]
    public float volume = .75f;

    [Range(0f, 1f)]
    public float volumeVariance = .1f;

    [Range(.1f, 3f)]
    public float pitch = 1f;

    [Range(0f, 1f)]
    public float pitchVariance = .1f;


    [SerializeField]
    AudioMixer audioMixer;


    float baseVolume;
    float basePitch;


    public AudioMixer AudioMixer
    {
        get => audioMixer;
        set => audioMixer = value;
    }




    protected virtual void StartBehaviour()
    {
        if (!source)
        {
            Debug.LogError($"{gameObject.name} missing audio source");
        }
    }


    protected virtual void Initialise()
    {
        if (source == null && gameObject.TryGetComponent(out source))
        {
            // print("Auto found audio:" + source.clip);
        }

        if (soundManager != null)
        {
            soundManager = SoundManager.current;
        }

        baseVolume = volume;
        basePitch = pitch;
        if (source)
        {
            source.volume = baseVolume;
            source.pitch = basePitch;
        }
    }

    public override bool IsPlaying()
    {
        return source.isPlaying;
    }

    public override void Pause()
    {
        source.Pause();
    }

    public override void Resume()
    {
        source.UnPause();
    }

    [ContextMenu("Play")]
    public override void Play()
    {
        if (!source.isPlaying)
        {
            PlayF();
        }
    }

    [ContextMenu("PlayF")]
    public override void PlayF()
    {
        source.volume = baseVolume * (1f + UnityEngine.Random.Range(-volumeVariance / 2f, volumeVariance / 2f));
        source.pitch = basePitch * (1f + UnityEngine.Random.Range(-pitchVariance / 2f, pitchVariance / 2f));

        source.Play();
    }

    [ContextMenu("Stop")]
    public override void Stop()
    {
        source.Stop();
    }

    public void ModifySource()
    {
    }

    private void OnDisable()
    {
        if (source && source.playOnAwake)
        {
            source.Stop();
        }
    }

    public virtual void UpdateSourceClip(AudioSource audioSource)
    {
        string[] ignoreList = {"minVolume", "maxVolume", "rolloffFactor"};

        DuplicateObjectScript.CopyPropertiesTo(audioSource, source, new List<string>(ignoreList));
    }

    public virtual void SetSoundLocation(Vector3 position)
    {
        transform.position = position;
    }

    public virtual void TransferSound(Sound other)
    {
        source = other.source;
        volume = other.volume;
        volumeVariance = other.volumeVariance;
        pitch = other.pitch;
        pitchVariance = other.pitchVariance;
        audioMixer = other.AudioMixer;
    }

    public override AudioClip GetClip()
    {
        return source.clip;
    }

    public override void SetClip(AudioClip audioClip)
    {
        source.clip = audioClip;
    }
}