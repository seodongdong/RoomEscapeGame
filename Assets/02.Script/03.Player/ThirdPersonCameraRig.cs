using UnityEngine;
using System.Collections;

/// <summary>
/// 추격전 시 1인칭 ↔ 3인칭 카메라 자동 전환
///
/// [기획서 기준]
/// "기본 시점 : 1인칭 / 추격전 시 : 3인칭 모드로 자동 전환"
/// "시점 조작 : 마우스 움직임으로 돌림" (3인칭에서도 동일하게 유지)
///
/// [동작 원리]
/// 기존 Player.HandleMouseLook()의 회전 로직(좌우=캐릭터 본체, 상하=카메라)은
/// 그대로 유지됩니다. 이 스크립트는 카메라의 "로컬 위치"만
/// 1인칭 오프셋 ↔ 3인칭 오프셋(캐릭터 뒤쪽 위)으로 부드럽게 전환합니다.
/// 회전 주체는 바뀌지 않으므로 마우스 시점 조작 방식은 1인칭과 동일하게 유지됩니다.
///
/// [씬 배치]
/// Player의 자식인 카메라(Player.cs의 cameraTransform과 동일한 오브젝트)에 부착.
/// GameManager.OnStateChanged 이벤트를 구독해 GameState.Chase 진입/이탈을 감지합니다.
/// </summary>
public class ThirdPersonCameraRig : MonoBehaviour
{
	[Header("1인칭 오프셋 (기본값)")]
	[Tooltip("평상시 카메라의 로컬 위치 (보통 0,0,0에 가까움, 캐릭터 머리 위치)")]
	[SerializeField] private Vector3 firstPersonLocalPosition = Vector3.zero;

	[Header("3인칭 오프셋 (추격전)")]
	[Tooltip("추격전 시 카메라가 이동할 로컬 위치 (캐릭터 뒤쪽 위)")]
	[SerializeField] private Vector3 thirdPersonLocalPosition = new Vector3(0f, 1.2f, -3.5f);

	[Header("전환 속도")]
	[SerializeField] private float transitionDuration = 0.5f;
	[SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[Header("3인칭 모드에서 캐릭터 메시 표시")]
	[Tooltip("1인칭에서는 보통 꺼져 있는 캐릭터 메시를 3인칭일 때 켜야 합니다.")]
	[SerializeField] private Player player;

	private Coroutine _transitionCoroutine;
	private bool _isThirdPerson = false;

	public bool IsThirdPerson => _isThirdPerson;

	private void Awake()
	{
		// 시작 시 1인칭 위치로 초기화
		transform.localPosition = firstPersonLocalPosition;

		if (player == null)
			player = GetComponentInParent<Player>();
	}

	private void Start()
	{
		if (GameManager.Instance != null)
			GameManager.Instance.OnStateChanged += HandleStateChanged;
		else
			Debug.LogWarning("[ThirdPersonCameraRig] GameManager.Instance가 null입니다. 추격전 카메라 전환이 동작하지 않습니다.");
	}

	private void OnDestroy()
	{
		if (GameManager.Instance != null)
			GameManager.Instance.OnStateChanged -= HandleStateChanged;
	}

	private void HandleStateChanged(GameState newState)
	{
		bool shouldBeThirdPerson = (newState == GameState.Chase);

		if (shouldBeThirdPerson == _isThirdPerson) return; // 이미 같은 모드면 무시

		_isThirdPerson = shouldBeThirdPerson;

		if (_transitionCoroutine != null)
			StopCoroutine(_transitionCoroutine);

		_transitionCoroutine = StartCoroutine(TransitionTo(_isThirdPerson));

		SetPlayerMeshVisible(_isThirdPerson);

		Debug.Log($"[ThirdPersonCameraRig] {(_isThirdPerson ? "3인칭 전환" : "1인칭 복귀")} (상태: {newState})");
	}

	private IEnumerator TransitionTo(bool toThirdPerson)
	{
		Vector3 startPos = transform.localPosition;
		Vector3 endPos = toThirdPerson ? thirdPersonLocalPosition : firstPersonLocalPosition;

		float elapsed = 0f;
		while (elapsed < transitionDuration)
		{
			elapsed += Time.deltaTime;
			float t = transitionCurve.Evaluate(Mathf.Clamp01(elapsed / transitionDuration));
			transform.localPosition = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}

		transform.localPosition = endPos;
	}

	/// <summary>
	/// 1인칭에서는 보통 자기 캐릭터 메시가 카메라에 가려 안 보이도록 꺼두는 경우가 많아,
	/// 3인칭으로 전환될 때만 플레이어 메시를 켜줍니다.
	/// (CameraPuzzleBase의 SetPlayerMeshVisible과 동일한 목적의 별도 구현 — 퍼즐 시스템에 의존하지 않기 위해 분리)
	/// </summary>
	private void SetPlayerMeshVisible(bool visible)
	{
		if (player == null) return;
		foreach (var m in player.GetComponentsInChildren<MeshRenderer>())
			m.enabled = visible;
		foreach (var m in player.GetComponentsInChildren<SkinnedMeshRenderer>())
			m.enabled = visible;
	}
}