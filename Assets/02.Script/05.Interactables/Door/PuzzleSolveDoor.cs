using UnityEngine;
using System.Collections;

/// <summary>
/// 퍼즐 완료 후 열리는 문. 열린 뒤에는 여닫기가 자유롭습니다.
///
/// [동작]
/// 퍼즐 미완료: "[F] ..." 프롬프트 + 잠김 대사
/// 퍼즐 완료 후 닫힌 상태: "[F] 문 열기" → 문 열림
/// 퍼즐 완료 후 열린 상태: "[F] 문 닫기" → 문 닫힘
///
/// [목각인형 지급]
/// 문을 통과(OnTriggerEnter)할 때 한 번만 지급.
/// 방에 들어갔다 나올 때마다 중복 지급되지 않습니다.
/// </summary>
public class PuzzleSolveDoor : MonoBehaviour, IInteractable
{
	[Header("퍼즐 연결")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("문 오브젝트")]
	[Tooltip("문 모델 오브젝트 (자식 오브젝트에 있을 경우 연결)")]
	[SerializeField] private Transform doorTransform;
	[Tooltip("Animator가 있으면 자동으로 사용. 없으면 Position 이동 방식 사용.")]
	[SerializeField] private Animator doorAnimator;
	[Tooltip("열릴 때 이동할 거리/방향 (Animator 없을 때 사용)")]
	[SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0);
	[SerializeField] private float openDuration = 0.6f;

	[Tooltip("true: 퍼즐 완료 시 자동으로 열림 (작은 방 문용)\nfalse: 잠금만 해제, 플레이어가 직접 열어야 함 (출구용)")]
	[SerializeField] private bool autoOpen = true;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[Tooltip("퍼즐 미완료 시 상호작용 프롬프트")]
	[SerializeField] private string lockedPrompt = "[F] 문 (잠김)";
	[TextArea(2, 4)]
	[SerializeField] private string lockedDialogue = "잠겨있다...";
	[TextArea(2, 4)]
	[SerializeField] private string openDialogue = "문이 열렸다.";
	[TextArea(2, 4)]
	[SerializeField] private string closeDialogue = "";  // 비워두면 대사 없음

	[Header("목각인형 지급 (선택)")]
	[SerializeField] private string woodenDollId = "";
	[SerializeField] private string woodenDollName = "나무인형";
	[TextArea(1, 2)]
	[SerializeField] private string woodenDollDialogue = "나무로 만든 인형이다.";
	[SerializeField] private GameObject woodenDollPrefab;

	// ── 런타임 상태 ──────────────────────────────────────────
	private IPuzzle _puzzle;
	private bool _puzzleSolved = false;
	private bool _isOpen = false;
	private bool _isAnimating = false;
	private bool _dollGiven = false;
	private Vector3 _closedPosition;
	private Transform _doorTarget;   // doorTransform 없으면 this.transform 사용

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

		var ui = FindAnyObjectByType<UIManager>();

		if (!_puzzleSolved)
		{
			ui?.ShowDialogue(speaker, lockedDialogue);
			return;
		}

		if (_isOpen)
		{
			// 열린 문 → 닫기
			if (!string.IsNullOrEmpty(closeDialogue))
				ui?.ShowDialogue(speaker, closeDialogue);
			StartCoroutine(AnimateDoor(false));
		}
		else
		{
			// 닫힌 문 → 열기
			ui?.ShowDialogue(speaker, openDialogue);
			StartCoroutine(AnimateDoor(true));
		}
	}

	// ── 자유 출입 해제 (Stage2_ExitTrigger에서 호출) ────────

	/// <summary>
	/// 첫 퇴장 후 호출. 이후 문을 자유롭게 여닫을 수 있게 잠금 상태를 완전히 해제합니다.
	/// 프롬프트가 "문 열기 / 문 닫기"로 바뀌고 목각인형 지급 여부와 무관하게 출입 가능.
	/// </summary>
	public void UnlockFreeAccess()
	{
		_puzzleSolved = true;  // 이미 true겠지만 확실히
		_isOpen = false; // 현재 닫힌 상태로 간주
		Debug.Log("[PuzzleSolveDoor] 자유 출입 해제됨");
	}

	// ── 퍼즐 완료 콜백 ───────────────────────────────────────

	private void OnPuzzleSolvedHandler()
	{
		_puzzleSolved = true;
		var ui = FindAnyObjectByType<UIManager>();

		if (autoOpen)
		{
			// 퍼즐 완료 시 자동으로 열림 (작은 방 문)
			ui?.ShowDialogue(speaker, openDialogue);
			StartCoroutine(AnimateDoor(true));
		}
		else
		{
			// 잠금만 해제, 대사 없음 (출구 문 — 플레이어가 직접 F키로 열어야 함)
			Debug.Log("[PuzzleSolveDoor] 퍼즐 완료 → 잠금 해제 (수동 열기)");
		}
	}

	// ── 문 애니메이션 ─────────────────────────────────────────

	private IEnumerator AnimateDoor(bool opening)
	{
		_isAnimating = true;
		_isOpen = opening;

		if (doorAnimator != null)
		{
			doorAnimator.SetBool("IsOpen", opening);
			yield return new WaitForSeconds(openDuration);
		}
		else
		{
			Vector3 startPos = _doorTarget.position;
			Vector3 targetPos = opening
				? _closedPosition + openOffset
				: _closedPosition;

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

	// ── 목각인형 지급 (방 통과 시 한 번만) ────────────────────

	private void OnTriggerEnter(Collider other)
	{
		if (_dollGiven || string.IsNullOrEmpty(woodenDollId)) return;
		if (!other.CompareTag("Player")) return;
		if (!_puzzleSolved) return;

		var player = other.GetComponent<Player>();
		if (player == null) return;
		if (player.Inventory.HasItem(woodenDollId)) return;

		_dollGiven = true;

		// PlayerInventory 등록 (IItem 인터페이스용)
		var clueItem = new ClueItem(woodenDollId, woodenDollName, woodenDollDialogue);
		player.Inventory.AddItem(clueItem);
		GameManager.Instance?.ClueTracker.RegisterClue(woodenDollId);

		// InventoryUI_Complete 등록 (UI 표시용)
		// itemPrefab 연결 → 인벤토리에서 "3D로 보기" 버튼 활성화
		var inventoryData = new InventoryItemData
		{
			itemId = woodenDollId,
			title = woodenDollName,
			description = woodenDollDialogue,
			itemType = ItemType.UsableItem,
			itemPrefab = woodenDollPrefab  // ← Inspector에서 연결한 프리팹
		};
		var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
		inventoryUI?.AddItem(inventoryData);

		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, woodenDollDialogue);

		Debug.Log($"[PuzzleSolveDoor] 목각인형 지급: {woodenDollName}");
	}
}