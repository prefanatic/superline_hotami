using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public float deferredRate = 0.2f;
    public float minPitchClamp = 0.3f;

    public int sfxVolumeOffset = 0;
    public int musicVolumeOffset = 0;

    public Sound[] sounds;

    private float deferredPitch = 1f;

    public static AudioController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        foreach (var sound in sounds)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = sound.clip;
            source.volume = sound.volume;
            source.loop = sound.loop;
            sound.source = source;
        }
    }

    void Update()
    {
        // Total jank
        musicVolumeOffset = Mathf.Clamp(musicVolumeOffset, -5, 5);
        sfxVolumeOffset = Mathf.Clamp(sfxVolumeOffset, -5, 5);


        // Track timescale and adjust pitch
        var exactPitch = Mathf.Clamp(Time.timeScale, minPitchClamp, 1f);
        deferredPitch = Mathf.Lerp(deferredPitch, exactPitch, deferredRate);
        foreach (var sound in sounds)
        {
            var source = sound.source;
            source.pitch = deferredPitch;

            var volume = sound.volume;
            if (sound.music)
            {
                volume += (musicVolumeOffset / 10f);
            }
            else
            {
                volume += (sfxVolumeOffset / 10f);
            }
            source.volume = volume;
        }
    }

    public void Play(string name)
    {
        Sound sound = null;
        foreach (var s in sounds)
        {
            if (s.name == name)
            {
                sound = s;
                break;
            }
        }
        if (sound == null) return;

        sound.source.Play();
    }

}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public float volume;
    public bool music = false;
    public bool loop = false;
    [HideInInspector]
    public AudioSource source;
}