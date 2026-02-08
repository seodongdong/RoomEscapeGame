using UnityEngine;

/// <summary>
/// 오디오 관리 인터페이스
/// BGM, 효과음, 발소리 재생 및 볼륨 조절
/// </summary>
public interface IAudioManager
{
	void PlayBGM(string bgmId);
	void StopBGM();
	void PlaySFX(string sfxId);
	void PlayFootstep();
	void SetBGMVolume(float volume);
	void SetSFXVolume(float volume);
}