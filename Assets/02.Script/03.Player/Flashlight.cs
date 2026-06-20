using UnityEngine;

/// <summary>
/// 손전등 On/Off 시스템
///
/// [기획서 기준]
/// - 3스테이지 미로 입장 시 TV장에서 발견 (FlashlightPickup이 Acquire() 호출)
/// - 획득 후에는 게임 내내(다른 스테이지 포함) O 또는 L키로 On/Off 가능
/// - 획득 전에는 토글 입력을 받아도 아무 동작 안 함
///
/// [씬 배치]
/// Player 오브젝트의 자식으로 Light 컴포넌트를 가진 오브젝트에 부착.
/// 카메라 앞 방향을 비추도록 Light(Spot 추천)의 위치/각도를 맞춰주세요.
///
/// [영속성]
/// Player와 함께 DontDestroyOnLoad 되거나, 씬마다 재배치된 Player에
/// 동일하게 붙어있다는 전제. 스테이지 간 보유 여부는 SaveSystem 작업에서
/// GameData에 포함되도록 추후 연동 예정(현재 작업 범위 밖).
/// </summary>
public class Flashlight : MonoBehaviour
{
	[Header("손전등 라이트")]
	[Tooltip("손전등 역할을 하는 Light 컴포넌트. 보통 Spot Light.")]
	[SerializeField] private Light flashlightLight;

	[Header("효과음 (선택)")]
	[SerializeField] private string toggleOnSfx = "flashlight_on";
	[SerializeField] private string toggleOffSfx = "flashlight_off";

	// ── 상태 ─────────────────────────────────────────────────
	private bool _hasFlashlight = false;
	private bool _isOn = false;

	public bool HasFlashlight => _hasFlashlight;
	public bool IsOn => _isOn;

	private void Awake()
	{
		// 시작 시 항상 꺼진 상태로 초기화
		if (flashlightLight != null)
			flashlightLight.enabled = false;
	}

	/// <summary>
	/// 손전등 획득. FlashlightPickup.Interact()에서 호출.
	/// 획득 즉시 자동으로 켜지지는 않음(기획서: On/Off는 플레이어가 직접 토글).
	/// </summary>
	public void Acquire()
	{
		if (_hasFlashlight) return; // 중복 획득 방지
		_hasFlashlight = true;
		Debug.Log("[Flashlight] 손전등 획득 — 이후 O/L 키로 사용 가능");
	}

	/// <summary>
	/// O 또는 L키 입력 시 Player.cs에서 호출.
	/// 손전등을 아직 획득하지 못했으면 아무 동작도 하지 않음.
	/// </summary>
	public void Toggle()
	{
		if (!_hasFlashlight) return;
		if (flashlightLight == null) return;

		_isOn = !_isOn;
		flashlightLight.enabled = _isOn;

		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX(_isOn ? toggleOnSfx : toggleOffSfx);

		Debug.Log($"[Flashlight] {(_isOn ? "켜짐" : "꺼짐")}");
	}
}