using UnityEngine;

/// <summary>
/// Stage1 크리처: 인형
/// TV 4회 시청 후 등장
/// - 첫 상호작용: "..................." → "날 해치려는 존재는 아닌 것 같다."
/// - 이후: 플레이어 주변 맴돌기 (아무 역할 없음)
/// </summary>
public class Stage1_DollCreature : MonoBehaviour, IInteractable
{
	[Header("Dialogue")]
	[SerializeField] private string creatureSpeaker = "인형";
	[TextArea(2, 5)]
	[SerializeField] private string creatureDialogue = "....................";

	[SerializeField] private string playerSpeaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string playerDialogue = "날 해치려는 존재는 아닌 것 같다.";

	[Header("Follow Settings")]
	[SerializeField] private float followDistance = 3f;     // 플레이어로부터 유지 거리
	[SerializeField] private float moveSpeed = 2f;          // 이동 속도
	[SerializeField] private float rotationSpeed = 3f;      // 회전 속도

	private bool _hasInteracted = false;
	private bool _shouldFollow = false;
	private Transform _player;

	private void Start()
	{
		var playerObj = FindAnyObjectByType<Player>();
		if (playerObj != null)
			_player = playerObj.transform;
	}

	public string InteractionPrompt
	{
		get
		{
			if (_hasInteracted) return "";  // 한 번 상호작용 후엔 프롬프트 없음
			return "[F] 조사하기";
		}
	}

	public bool CanInteract(IPlayer player)
	{
		return !_hasInteracted;
	}

	public void Interact(IPlayer player)
	{
		if (_hasInteracted) return;

		_hasInteracted = true;
		StartCoroutine(FirstInteractionSequence());
	}

	private System.Collections.IEnumerator FirstInteractionSequence()
	{
		var uiManager = FindAnyObjectByType<UIManager>();
		Debug.Log("[DollCreature] 첫 상호작용 시작");

		// 크리처 대사: "..................."
		Debug.Log("[DollCreature] 크리처 대사 표시");
		uiManager?.ShowDialogue(creatureSpeaker, creatureDialogue);

		// ⭐ 2초 자동 대기
		yield return new WaitForSeconds(2f);

		Debug.Log("[DollCreature] 이전 대사 닫기");
		uiManager?.HideDialogue();

		// ⭐ 잠깐 대기 (UI 갱신 시간)
		yield return new WaitForSeconds(0.3f);

		Debug.Log("[DollCreature] 플레이어 대사 표시");

		// 플레이어 반응: "날 해치려는 존재는 아닌 것 같다."
		uiManager?.ShowDialogue(playerSpeaker, playerDialogue);

		// ⭐ 2초 자동 대기
		yield return new WaitForSeconds(2f);

		Debug.Log("[DollCreature] 대사 종료");
		uiManager?.HideDialogue();

		// ⭐ 바로 플레이어 추적 시작
		_shouldFollow = true;
		Debug.Log("[DollCreature] 플레이어 추적 시작");
	}

	private void Update()
	{
		if (!_shouldFollow || _player == null) return;

		FollowPlayer();
	}

	private void FollowPlayer()
	{
		// 플레이어와의 거리 계산
		Vector3 targetPosition = _player.position;
		float distance = Vector3.Distance(transform.position, targetPosition);

		// 일정 거리 이상이면 천천히 다가가기
		if (distance > followDistance)
		{
			Vector3 direction = (targetPosition - transform.position).normalized;
			direction.y = 0; // 수평 이동만

			transform.position += direction * moveSpeed * Time.deltaTime;

			// 플레이어 방향으로 회전
			if (direction.magnitude > 0.1f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(direction);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
			}
		}
		// 거리 유지하면서 플레이어 바라보기
		else
		{
			Vector3 lookDirection = _player.position - transform.position;
			lookDirection.y = 0;

			if (lookDirection.magnitude > 0.1f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_player != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(_player.position, followDistance);
		}

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, 0.5f);
	}
}