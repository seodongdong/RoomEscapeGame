using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour, IAudioManager
{
    [System.Serializable]
    public class Sound
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Lists")]
    [SerializeField] private List<Sound> bgmList;
    [SerializeField] private List<Sound> sfxList;
    [SerializeField] private List<AudioClip> footstepClips;

    private Dictionary<string, Sound> _bgmDict;
    private Dictionary<string, Sound> _sfxDict;

    private void Awake()
    {
        _bgmDict = new Dictionary<string, Sound>();
        _sfxDict = new Dictionary<string, Sound>();

        foreach (var sound in bgmList)
        {
            _bgmDict[sound.id] = sound;
        }

        foreach (var sound in sfxList)
        {
            _sfxDict[sound.id] = sound;
        }
    }

    public void PlayBGM(string bgmId)
    {
        if (_bgmDict.TryGetValue(bgmId, out var sound))
        {
            bgmSource.clip = sound.clip;
            bgmSource.volume = sound.volume;
            bgmSource.loop = sound.loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM not found: {bgmId}");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string sfxId)
    {
        if (_sfxDict.TryGetValue(sfxId, out var sound))
        {
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
        else
        {
            Debug.LogWarning($"SFX not found: {sfxId}");
        }
    }

    public void PlayFootstep()
    {
        if (footstepClips.Count > 0)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Count)];
            sfxSource.PlayOneShot(clip, 0.5f);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}