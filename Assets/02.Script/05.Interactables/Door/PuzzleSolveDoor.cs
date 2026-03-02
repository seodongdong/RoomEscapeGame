using UnityEngine;
using System.Collections;

/// <summary>
/// 퍼즐 해결 시 열리는 출구 문
/// 퍼즐 풀기 전: 잠김 / 퍼즐 풀면: 자동으로 열림
/// 씬 전환 없음 - 복도로 그냥 나가는 문
/// </summary>
public class PuzzleSolvedDoor : MonoBehaviour, IInteractable
{
	[Header("연결 퍼즐")]
	[SerializeField] private MonoBehaviour puzzleObject; // IPuzzle 구현한 퍼즐

	[Header("문 설정")]
	[SerializeField] private Animator doorAnimator;       // 애니메이터 있으면 연결
	[SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0); // 애니메이터 없을 때 이동 방향
	[SerializeField] private float openDuration = 1f;    // 문 열리는 시간

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "퍼즐을 풀어야 열릴 것 같다...";
	[TextArea(2, 5)]
	[SerializeField] private string openDialogue = "문이 열렸다!";

	private IPuzzle _puzzle;
	private bool _isOpen = false;
	private Vector3 _closedPosition;

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;
		_closedPosition = transform.position;

		if (_puzzle == null && puzzleObject != null)
			Debug.LogError($"[PuzzleSolvedDoor] {puzzleObject.name}은 IPuzzle을 구현하지 않습니다!");
	}

	private void Start()
	{
		// 퍼즐 해결 이벤트 구독
		if (_puzzle != null)
		{
			_puzzle.OnPuzzleSolved += OnPuzzleSolved;
		}
	}

	private void OnDestroy()
	{
		if (_puzzle != null)
			_puzzle.OnPuzzleSolved -= OnPuzzleSolved;
	}

	// 퍼즐 해결 시 자동 호출
	private void OnPuzzleSolved()
	{
		OpenDoor();
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, openDialogue);

		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("door_unlock");

		Debug.Log("[PuzzleSolvedDoor] 퍼즐 해결 → 문 열림!");
	}

	public string InteractionPrompt
	{
		get
		{
			if (_isOpen) return "[F] 나가기";
			if (_puzzle != null && _puzzle.IsSolved) return "[F] 문 열기";
			return "[F] 문 (잠김)";
		}
	}

	public bool CanInteract(IPlayer player)
	{
		return true; // 항상 상호작용 가능 (잠김 대사 출력을 위해)
	}

	public void Interact(IPlayer player)
	{
		// 이미 열려있으면 통과 (콜라이더 제거로 처리)
		if (_isOpen) return;

		// 퍼즐 안 풀었으면 잠김 대사
		if (_puzzle != null && !_puzzle.IsSolved)
		{
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue(speaker, lockedDialogue);
			return;
		}

		// 퍼즐 풀었는데 문이 아직 안 열렸으면 열기
		if (_puzzle != null && _puzzle.IsSolved && !_isOpen)
		{
			OpenDoor();
		}
	}

	private void OpenDoor()
	{
		if (_isOpen) return;
		_isOpen = true;

		// 콜라이더 비활성화 (통과 가능하게)
		var col = GetComponent<Collider>();
		if (col != null) col.enabled = false;

		if (doorAnimator != null)
		{
			// 애니메이터 있으면 애니메이션
			doorAnimator.SetTrigger("Open");
		}
		else
		{
			// 없으면 위로 올라가는 효과
			StartCoroutine(SlideOpen());
		}
	}

	private void CloseDoor()
	{ 	if (!_isOpen) return;
		_isOpen = false;
		var col = GetComponent<Collider>();
		if (col != null) col.enabled = true;
		if (doorAnimator != null)
		{
			doorAnimator.SetTrigger("Close");
		}
		else
		{
			StartCoroutine(SlideClose());
		}
	}

	private IEnumerator SlideOpen()
	{
		Vector3 targetPosition = _closedPosition + openOffset;
		float elapsed = 0f;

		while (elapsed < openDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / openDuration;
			transform.position = Vector3.Lerp(_closedPosition, targetPosition, t);
			yield return null;
		}

		transform.position = targetPosition;
	}

	private IEnumerator SlideClose()
	{
		Vector3 startPosition = transform.position;
		float elapsed = 0f;
		while (elapsed < openDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / openDuration;
			transform.position = Vector3.Lerp(startPosition, _closedPosition, t);
			yield return null;
		}
		transform.position = _closedPosition;
	}

	private void OnDrawGizmos()
	{
		// 문이 열릴 위치 미리보기
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position + openOffset, transform.localScale);
		Gizmos.DrawLine(transform.position, transform.position + openOffset);
	}
}