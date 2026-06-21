using UnityEngine;

public class Flashlight : MonoBehaviour
{
	[Header("손전등 라이트")]
	[SerializeField] private Light flashlightLight;

	[Header("효과음 (선택)")]
	[SerializeField] private string toggleOnSfx = "flashlight_on";
	[SerializeField] private string toggleOffSfx = "flashlight_off";

	private bool _hasFlashlight = false;
	private bool _isOn = false;

	public bool HasFlashlight => _hasFlashlight;
	public bool IsOn => _isOn;

	private void Awake()
	{
		if (flashlightLight != null)
			flashlightLight.enabled = false;
	}

	public void Acquire()
	{
		if (_hasFlashlight) return;
		_hasFlashlight = true;
		Debug.Log("[Flashlight] 손전등 획득 — 이후 O/L 키로 사용 가능");
	}

	/// <summary>
	/// ★ 추가: 저장 데이터 복원 시 SaveLoader가 호출합니다.
	/// Acquire()와 달리 중복 체크 없이 바로 보유 상태로 설정합니다(불러오기 전용).
	/// </summary>
	public void RestoreHasFlashlight(bool hasFlashlight)
	{
		_hasFlashlight = hasFlashlight;
		// 손전등 On/Off 여부까지는 저장하지 않으므로, 불러온 직후는 항상 꺼진 상태로 시작합니다.
		_isOn = false;
		if (flashlightLight != null)
			flashlightLight.enabled = false;
	}

	public void Toggle()
	{
		if (!_hasFlashlight) return;
		if (flashlightLight == null) return;

		_isOn = !_isOn;
		flashlightLight.enabled = _isOn;

		var audioManager = GameServices.Audio;
		audioManager?.PlaySFX(_isOn ? toggleOnSfx : toggleOffSfx);

		Debug.Log($"[Flashlight] {(_isOn ? "켜짐" : "꺼짐")}");
	}
}