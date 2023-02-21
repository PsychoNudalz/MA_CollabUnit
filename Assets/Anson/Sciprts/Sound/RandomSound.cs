using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = Unity.Mathematics.Random;
/// <summary>
/// play random sound from pool of sound
/// added in Ver 9
/// </summary>
public class RandomSound : SoundAbstract
{
    // [Header("Ignore the things Above")]
    
    [SerializeField]
    private SoundAbstract[] sounds;

    [SerializeField]
    private AudioClip[] audioClips;

    private int seed = 0;

    
    
    protected override void StartBehaviour()
    {
        //base.StartBehaviour();
    }

    protected override void Initialise()
    {
        if (soundManager != null)
        {
            soundManager = SoundManager.current;
        }
        seed = (int) UnityEngine.Random.Range(0, 100f);
        if (sounds.Length == 0)
        {
            SetAllChildrenSounds();
        }
        
    }

    [ContextMenu("Bind Audio Clip to source")]
    public void BindAudioClipToSource()
    {
        for (int i = 0; i < Mathf.Min(audioClips.Length,sounds.Length); i++)
        {

            sounds[i].SetClip(audioClips[i]);
        }
    }

    [ContextMenu("Set all children sounds")]
    public void SetAllChildrenSounds()
    {
        List<SoundAbstract> temp = new List<SoundAbstract>();
        foreach (SoundAbstract s in GetComponentsInChildren<SoundAbstract>())
        {
            if (!s.Equals(this))
            {
                temp.Add(s);
            }
        }

        sounds = temp.ToArray();
    }

    public override void Pause()
    {
    }

    public override void Resume()
    {
    }

    public override bool IsPlaying()
    {
        return GetRandomSound().IsPlaying();
    }

    [ContextMenu("Play")]
    public override void Play()
    {
        GetRandomSound().Play();
        
    }
    [ContextMenu("PlayF")]

    public override void PlayF()
    {
        GetRandomSound().PlayF();        
    }
    [ContextMenu("Stop")]

    public override void Stop()
    {
        GetRandomSound().Stop();
    }

    /// <summary>
    /// Gets a random sound, will prioritize getting sounds that is not playing
    /// </summary>
    /// <returns></returns>
    SoundAbstract GetRandomSound()
    {
        // seed++;
        // seed = seed % sounds.Length;
        //
        seed = UnityEngine.Random.Range(0, sounds.Length );
        SoundAbstract temp = sounds[seed];
        for (int i = 0; i < sounds.Length; i++)
        {
            temp = sounds[(seed + i) % sounds.Length];
            if (!temp.IsPlaying() )
            {
                print(temp.name);
                return temp;
            }
        }
        return temp;
    } 
}
