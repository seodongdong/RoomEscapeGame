using UnityEngine;
using System.Collections;

/// <summary>
/// 2스테이지: 검은 인영 크리처 (쫄쫄이 입은 인영)
///
/// [기획서 행동 명세]
/// - 퍼즐 전: 맵 좌측 상단 작은 방 앞에 가만히 서서 플레이어를 빤히 응시
/// - 상호작용 불가 (IInteractable 미구현)
/// - 퍼즐 해결 후: 맵 뒤쪽 중앙으로 즉시 이동 → 작은 방 진입 가능
/// - 스테이지 퇴장 시: 입만 활짝 웃고 있는 연출 (납치 성공 암시)
///
/// [점프스케어 - Stage2_JumpscareTrigger와 연동]
/// 퍼즐 완료 후 관 앞에서 뒤를 돌아보면 방석에 크리처 인영 + 큰 박수소리
/// </summary>
public class Stage2_ShadowCreature : CreatureBase
{
	[Header("위치 설정")]
	[SerializeField] private Transform initialPosition;   // 작은 방 앞 (시작 위치)
	[SerializeField] private Transform finalPosition;     // 맵 뒤쪽 중앙 (퍼즐 해결 후)

	[Header("이동 설정")]
	[SerializeField] private float moveToFinalDuration = 0f;   // 0이면 순간 이동, 0 초과면 Lerp 이동

	[Header("퇴장 연출 (웃음)")]
	[SerializeField] private bool enableExitSmile = true;
	[SerializeField] private GameObject smileFaceObject;        // 웃는 표정 오브젝트 (활성화로 표현)
	[SerializeField] private GameObject normalFaceObject;       // 기본 표정 오브젝트

	[Header("응시 설정")]
	[SerializeField] private bool alwaysLookAtPlayer = true;    // 항상 플레이어 응시

	private bool _hasMoved = false;
	private bool _isMovingToFinal = false;

	// ─────────────────────────────────────────
	// 초기화
	// ─────────────────────────────────────────

	protected override void Start()
	{
		base.Start();

		// 시작 위치 설정
		if (initialPosition != null)
			transform.position = initialPosition.position;

		// 기본 표정 활성
		SetFaceExpression(false);
	}

	// ─────────────────────────────────────────
	// 매 프레임 행동
	// ─────────────────────────────────────────

	protected override void UpdateBehavior()
	{
		// 항상 플레이어 응시 (상호작용 없이 빤히 바라보기만 함)
		if (alwaysLookAtPlayer && _player != null)
		{
			Vector3 lookDir = _player.transform.position - transform.position;
			lookDir.y = 0f;   // Y축 회전만
			if (lookDir.sqrMagnitude > 0.001f)
				transform.rotation = Quaternion.LookRotation(lookDir);
		}
	}

	// ─────────────────────────────────────────
	// 퍼즐 해결 후 위치 이동 (AltarCandyPuzzle에서 호출)
	// ─────────────────────────────────────────

	/// <summary>
	/// 퍼즐 해결 후 맵 뒤쪽 중앙으로 이동합니다.
	/// 이동 후 작은 방 진입이 가능해집니다.
	/// </summary>
	public void MoveToFinalPosition()
	{
		if (_hasMoved) return;
		_hasMoved = true;

		if (finalPosition == null)
		{
			Debug.LogWarning("[ShadowCreature] finalPosition이 설정되지 않았습니다.");
			return;
		}

		if (moveToFinalDuration <= 0f)
		{
			// 순간 이동 (기획서: 퍼즐 화면에서 나오면 이미 이동해 있음)
			transform.position = finalPosition.position;
			Debug.Log("[ShadowCreature] 작은 방 → 맵 뒤쪽 중앙 이동 완료");
		}
		else
		{
			StartCoroutine(MoveToFinalCoroutine());
		}
	}

	private IEnumerator MoveToFinalCoroutine()
	{
		_isMovingToFinal = true;
		Vector3 start = transform.position;
		Vector3 end = finalPosition.position;
		float elapsed = 0f;

		while (elapsed < moveToFinalDuration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / moveToFinalDuration);
			transform.position = Vector3.Lerp(start, end, t);
			yield return null;
		}

		transform.position = end;
		_isMovingToFinal = false;
		Debug.Log("[ShadowCreature] 이동 완료");
	}

	// ─────────────────────────────────────────
	// 퇴장 시 웃음 연출
	// ─────────────────────────────────────────

	/// <summary>
	/// 플레이어 퇴장 시 호출 (Stage2_ExitTrigger 또는 씬에서 호출)
	/// 기획서: "아저씨는 상호작용 불가이지만, 입만 활짝 웃고 있는걸로"
	/// </summary>
	public void TriggerExitSmile()
	{
		if (!enableExitSmile) return;
		SetFaceExpression(true);
		Debug.Log("[ShadowCreature] 퇴장 웃음 연출");
	}

	private void SetFaceExpression(bool isSmiling)
	{
		if (normalFaceObject != null)
			normalFaceObject.SetActive(!isSmiling);
		if (smileFaceObject != null)
			smileFaceObject.SetActive(isSmiling);
	}

	// ─────────────────────────────────────────
	// 점프스케어 오버라이드 (2스테이지용 없음)
	// ─────────────────────────────────────────

	protected override void TriggerJumpscare()
	{
		// 2스테이지 인영 크리처는 직접 점프스케어 없음
		// 퍼즐 완료 후 뒤돌아볼 때의 연출은 Stage2_JumpscareTrigger가 담당
	}
}