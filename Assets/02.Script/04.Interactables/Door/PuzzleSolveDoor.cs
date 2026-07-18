using UnityEngine;
using System.Collections;

/// <summary>
/// 퍼즐 완료 후 열리는 문. 열린 뒤에는 여닫기가 자유롭습니다.
/// </summary>
public class PuzzleSolveDoor : MonoBehaviour, IInteractable, ISaveableObject
{
	[Header("퍼즐 연결")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("문 오브젝트")]
	[SerializeField] private Transform doorTransform;
	[SerializeField] private Animator doorAnimator;
	[SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0);
	[SerializeField] private float openDuration = 0.6f;
	[SerializeField] private bool autoOpen = false;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[SerializeField] private string lockedPrompt = "[F] 문 (잠김)";
	[TextArea(2, 4)][SerializeField] private string lockedDialogue = "잠겨있다...";
	[TextArea(2, 4)][SerializeField] private string openDialogue = "문이 열렸다.";
	[TextArea(2, 4)][SerializeField] private string closeDialogue = "";

	[Header("목각인형 지급 (선택)")]
	[SerializeField] private string woodenDollId = "";
	[SerializeField] private string woodenDollName = "나무인형";
	[TextArea(1, 2)][SerializeField] private string woodenDollDialogue = "나무로 만든 인형이다.";
	[SerializeField] private GameObject woodenDollPrefab;

	[Header("저장 ID (씬 내 유일해야 함)")]
	[SerializeField] private string saveId = "puzzledoor_001";

	// ── 런타임 상태 ──────────────────────────────────────────
	private IPuzzle _puzzle;
	private bool _puzzleSolved = false;
	private bool _isOpen = false;
	private bool _isAnimating = false;
	private bool _dollGiven = false;
	private Vector3 _closedPosition;
	private Transform _doorTarget;

	// ── ISaveableObject ───────────────────────────────────────

	public string SaveId => saveId;

	[System.Serializable]
	private class PuzzleDoorState
	{
		public bool puzzleSolved;
		public bool isOpen;
		public bool dollGiven;
	}

	public string SaveState()
	{
		return JsonUtility.ToJson(new PuzzleDoorState
		{
			puzzleSolved = _puzzleSolved,
			isOpen = _isOpen,
			dollGiven = _dollGiven
		});
	}

	public void LoadState(string json)
	{
		if (string.IsNullOrEmpty(json)) return;
		var state = JsonUtility.FromJson<PuzzleDoorState>(json);

		_dollGiven = state.dollGiven;
		_puzzleSolved = state.puzzleSolved;

		if (state.isOpen)
			// ★ 애니메이션 없이 즉시 열린 상태로 (복원이므로 연출 불필요)
			StartCoroutine(AnimateDoor(true));
	}

	// ── 초기화 ────────────────────────────────────────────────

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;
		_doorTarget = doorTransform != null ? doorTransform : transform;
		_closedPosition = _doorTarget.position;
	}

	private void Start()
	{
		if (_puzzle != null)
			_puzzle.OnPuzzleSolved += OnPuzzleSolvedHandler;
	}

	private void OnDestroy()
	{
		if (_puzzle != null)
			_puzzle.OnPuzzleSolved -= OnPuzzleSolvedHandler;
	}

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt
	{
		get
		{
			if (!_puzzleSolved) return lockedPrompt;
			return _isOpen ? "[F] 문 닫기" : "[F] 문 열기";
		}
	}

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		if (_isAnimating) return;

		var ui = GameServices.UI;

		if (!_puzzleSolved)
		{
			ui?.ShowDialogue(speaker, lockedDialogue);
			GameServices.Audio?.PlaySFX("door_locked");
			return;
		}

		if (_isOpen)
		{
			if (!string.IsNullOrEmpty(closeDialogue))
				ui?.ShowDialogue(speaker, closeDialogue);
			StartCoroutine(AnimateDoor(false));
		}
		else
		{
			ui?.ShowDialogue(speaker, openDialogue);
			GameServices.Audio?.PlaySFX("door_open");
			StartCoroutine(AnimateDoor(true));
		}
	}

	// ── 자유 출입 해제 ────────────────────────────────────────

	public void UnlockFreeAccess()
	{
		_puzzleSolved = true;
		_isOpen = false;
		Debug.Log("[PuzzleSolveDoor] 자유 출입 해제됨");
	}

	// ── 퍼즐 완료 콜백 ───────────────────────────────────────

	private void OnPuzzleSolvedHandler()
	{
		_puzzleSolved = true;
		GameServices.Audio?.PlaySFX("door_unlock");

		if (autoOpen)
		{
			GameServices.UI?.ShowDialogue(speaker, openDialogue);
			StartCoroutine(AnimateDoor(true));
		}
		else
		{
			Debug.Log("[PuzzleSolveDoor] 퍼즐 완료 → 잠금 해제 (수동 열기 대기)");
		}
	}

	// ── 문 애니메이션 ─────────────────────────────────────────

	private IEnumerator AnimateDoor(bool opening)
	{
		_isAnimating = true;
		_isOpen = opening;

		if (doorAnimator != null)
		{
			doorAnimator.SetTrigger(opening ? "Open" : "Close");
			yield return new WaitForSeconds(openDuration);
		}
		else
		{
			Vector3 startPos = _doorTarget.position;
			Vector3 targetPos = opening ? _closedPosition + openOffset : _closedPosition;

			float elapsed = 0f;
			while (elapsed < openDuration)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
				_doorTarget.position = Vector3.Lerp(startPos, targetPos, t);
				yield return null;
			}
			_doorTarget.position = targetPos;
		}

		_isAnimating = false;
	}

	// ── 목각인형 지급 ─────────────────────────────────────────

	private void OnTriggerEnter(Collider other)
	{
		if (_dollGiven || string.IsNullOrEmpty(woodenDollId)) return;
		if (!other.CompareTag("Player")) return;
		if (!_puzzleSolved) return;

		var player = other.GetComponent<Player>();
		if (player == null) return;
		if (player.Inventory.HasItem(woodenDollId)) return;

		_dollGiven = true;

		ClueRegistrar.RegisterUsableItem(player, woodenDollId, woodenDollName, "", woodenDollDialogue, woodenDollPrefab);
		GameServices.UI?.ShowDialogue(speaker, woodenDollDialogue);
		Debug.Log($"[PuzzleSolveDoor] 목각인형 지급: {woodenDollName}");
	}
}