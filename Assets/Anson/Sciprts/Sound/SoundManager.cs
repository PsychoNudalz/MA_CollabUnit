using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 11th generation of the sound manager
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager current;
 
    [SerializeField] List<SoundAbstract> sounds = new List<SoundAbstract>();
    [SerializeField] List<SoundAbstract> soundsCache = new List<SoundAbstract>();
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] bool playBGM = true;
    [SerializeField] SoundAbstract bgm;

    private void Awake()
    {
        if (current)
        {
            Destroy(current);
        }
        current = this;
    }

    private void Start()
    {
        UpdateSounds();
        if (bgm != null && playBGM)
        {
            if (!bgm.IsPlaying())
            {
                bgm.Play();
            }
        }
    }


    [ContextMenu("Update Sounds")]
    public void UpdateSounds()
    {
        List<SoundAbstract> newSounds = new List<SoundAbstract>(FindObjectsOfType<SoundAbstract>());
        sounds = new List<SoundAbstract>();
        foreach (SoundAbstract s in newSounds)
        {
            AddSounds(s);
            s.SoundManager = this;

        }
    }

    public void AddSounds(SoundAbstract sa)
    {
        if (!sounds.Contains(sa))
        {
            if (sa is Sound item)
            {
                if (item.AudioMixer == null)
                {
                    item.AudioMixer = audioMixer;
                }

                sounds.Add(item);
            }
        }
    }

    public void PauseAllSounds()
    {
        UpdateSounds();
        soundsCache = new List<SoundAbstract>();
        foreach (SoundAbstract s in sounds)
        {
            if (s.IsPlaying())
            {
                soundsCache.Add(s);
                s.Pause();
            }
        }
    }

    public void ResumeSounds()
    {

        foreach (SoundAbstract s in soundsCache)
        {
            s.Resume();
        }
        UpdateSounds();
    }


    public void StopAllSounds()
    {
        soundsCache = new List<SoundAbstract>();
        foreach (SoundAbstract s in sounds)
        {
            if (s!= null && s.IsPlaying())
            {
                soundsCache.Add(s);
                s.Stop();
            }
        }
    }

    private void OnDestroy()
    {
        StopAllSounds();
    }
}
