using UnityEngine;

// 오디오 매니저 인터페이스

public interface IAudioManager
{
    void PlayBGM(string bgmId);
    void StopBGM();
    void PlaySFX(string sfxId);
    void PlayFootstep();
    void SetBGMVolume(float volume);
    void SetSFXVolume(float volume);
}
