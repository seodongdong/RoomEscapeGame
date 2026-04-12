using UnityEngine;
using System.Collections;

/// <summary>
/// 1스테이지: 인형 크리처 (개선판)
///
/// [기획서 내용]
/// - TV 4번 시청 후 등장
/// - 상호작용하면 가오나시처럼 "..................."
/// - 이후 주인공 주변을 맴돌며 딱히 역할 없음
/// - 플레이어 반응: 첫 발견 "!!" / 상호작용 후 "날 해치려는 존재는 아닌 것 같다."
///
/// [기존 코드 대비 추가된 기능]
/// - IInteractable 구현 → F키로 크리처 상호작용 가능
/// - 첫 발견 대사 자동 출력 (TriggerJumpscare 오버라이드)
/// - 상호작용 횟수별 대사 분기
/// - 주인공 주변 맴돌기 (CircleAround)
/// </summary>
public class Stage1_DollCreature : CreatureBase, IInteractable
{
	[Header("맴돌기 설정")]
	[SerializeField] private float orbitRadius = 2.5f;       // 맴도는 반경
	[SerializeField] private float orbitSpeed = 30f;          // 도는 속도 (도/초)
	[SerializeField] private float slowApproachSpeed = 1f;    // 처음 접근 속도

	[Header("접근 거리 제한")]
	[SerializeField] private float minStopDistance = 1.5f; // 플레이어와 최소 유지 거리

	[Header("상호작용 대사")]
	[SerializeField] private string creatureSpeaker = "???";
	[TextArea(2, 4)]
	[SerializeField] private string creatureDialogue = "....................";

	[SerializeField] private string playerSpeaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string firstReactionDialogue = "날 해치려는 존재는 아닌 것 같다.";
	[TextArea(2, 4)]
	[SerializeField] private string repeatDialogue = "...아무 반응이 없다.";

	[Header("발견 대사 (TVPlayer Step4에서 이미 출력 — 비워도 됨)")]
	[SerializeField] private string spotDialogue = "";  // 비워두면 출력 안 함

	private bool _isOrbiting = false;
	private float _orbitAngle = 0f;
	private int _interactCount = 0;

	// ─────────────────────────────────────────
	// IInteractable 구현
	// ─────────────────────────────────────────

	public string InteractionPrompt => "[F] 조사하기";

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();
		_interactCount++;

		if (_interactCount == 1)
		{
			// 크리처 반응: "..................."
			uiManager?.ShowDialogue(creatureSpeaker, creatureDialogue);

			// 잠시 후 플레이어 반응
			StartCoroutine(ShowPlayerReactionDelayed(uiManager, firstReactionDialogue, 2.5f));

			// 맴돌기 시작
			_isOrbiting = true;
		}
		else
		{
			uiManager?.ShowDialogue(creatureSpeaker, repeatDialogue);
		}
	}

	// ─────────────────────────────────────────
	// 행동 로직
	// ─────────────────────────────────────────

	protected override void UpdateBehavior()
	{
		if (_player == null) return;

		float dist = Vector3.Distance(transform.position, _player.transform.position);

		// ── 맴돌기 중일 때
		if (_isOrbiting)
		{
			_orbitAngle += orbitSpeed * Time.deltaTime;
			float rad = _orbitAngle * Mathf.Deg2Rad;

			Vector3 offset = new Vector3(
				Mathf.Sin(rad) * orbitRadius,
				0f,
				Mathf.Cos(rad) * orbitRadius
			);

			Vector3 targetPos = _player.transform.position + offset;
			targetPos.y = transform.position.y;

			transform.position = Vector3.MoveTowards(
				transform.position, targetPos, slowApproachSpeed * Time.deltaTime);
		}
		// ── 접근 중일 때: 최소 거리(minDistance) 이상 가까워지지 않도록
		else if (dist > minStopDistance)
		{
			Vector3 direction = (_player.transform.position - transform.position).normalized;
			transform.position += direction * slowApproachSpeed * Time.deltaTime;
		}

		// 항상 플레이어 바라보기
		Vector3 lookDir = _player.transform.position - transform.position;
		lookDir.y = 0f;
		if (lookDir.sqrMagnitude > 0.001f)
			transform.rotation = Quaternion.LookRotation(lookDir);
	}

	// ─────────────────────────────────────────
	// 첫 등장 연출 오버라이드
	// ─────────────────────────────────────────

	protected override void TriggerJumpscare()
	{
		// 기획서: 첫 등장 연출은 TVPlayer Step4에서 이미 처리
		// ("!!", "이게 뭐지...?" 대사 출력됨)
		// CreatureBase의 점프스케어(사라지기) 동작 막기
		_hasJumpscared = true; // 플래그만 세우고 사라지지 않음

		if (!string.IsNullOrEmpty(spotDialogue))
		{
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue(playerSpeaker, spotDialogue);
		}
	}

	// ─────────────────────────────────────────
	// 유틸
	// ─────────────────────────────────────────

	private IEnumerator ShowPlayerReactionDelayed(UIManager uiManager, string dialogue, float delay)
	{
		yield return new WaitForSeconds(delay);
		uiManager?.ShowDialogue(playerSpeaker, dialogue);
	}
}