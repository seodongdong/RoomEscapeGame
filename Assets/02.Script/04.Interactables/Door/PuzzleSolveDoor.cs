using UnityEngine;
using System.Collections;

/// <summary>
/// 퍼즐 완료 후 열리는 문. 열린 뒤에는 여닫기가 자유롭습니다.
///
/// [기획서 기준]
/// - 퍼즐 해결 시에만 열림. 자동 개방 없이 수동 상호작용 필요
/// - 퍼즐 해결 시 문 열리는 효과음 재생
///
/// [autoOpen 기본값 변경]
/// true  → 퍼즐 완료 즉시 자동 열림  (방 안쪽 → 복도 연결문 등)
/// false → 잠금만 해제, F키로 직접 열어야 함  ← 기획서 기본값
///
/// [동작]
/// 퍼즐 미완료 : lockedPrompt 프롬프트 + 잠김 대사
/// 퍼즐 완료 후 닫힌 상태 : "[F] 문 열기" → 열림
/// 퍼즐 완료 후 열린 상태 : "[F] 문 닫기" → 닫힘
///
/// [목각인형 지급]
/// 문을 통과(OnTriggerEnter)할 때 한 번만 지급.
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

	[Tooltip(
		"false(기본) : 잠금만 해제, 플레이어가 F키로 직접 열어야 함 ← 기획서 기준\n" +
		"true         : 퍼즐 완료 즉시 자동 열림 (작은 방 문 등 특수 케이스)")]
	[SerializeField] private bool autoOpen = false;   // ★ 기본값 false로 변경

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[Tooltip("퍼즐 미완료 시 상호작용 프롬프트")]
	[SerializeField] private string lockedPrompt = "[F] 문 (잠김)";
	[TextArea(2, 4)]
	[SerializeField] private string lockedDialogue = "잠겨있다...";
	[TextArea(2, 4)]
	[SerializeField] private string openDialogue = "문이 열렸다.";
	[TextArea(2, 4)]
	[SerializeField] private string closeDialogue = "";   // 비워두면 대사 없음

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
	private Transform _doorTarget;

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
			// 퍼즐 미완료 → 잠김 대사 + 효과음
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

	// ── 자유 출입 해제 (Stage2_ExitTrigger 등에서 호출) ──────

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

		// 효과음: 잠겼던 문이 열리는 소리
		GameServices.Audio?.PlaySFX("door_unlock");

		if (autoOpen)
		{
			// 특수 케이스: 자동 열림
			var ui = GameServices.UI;
			ui?.ShowDialogue(speaker, openDialogue);
			StartCoroutine(AnimateDoor(true));
		}
		else
		{
			// 기획서 기본: 잠금만 해제, 플레이어가 F키로 직접 열어야 함
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

		// PlayerInventory 등록
		var clueItem = new ClueItem(woodenDollId, woodenDollName, woodenDollDialogue);
		player.Inventory.AddItem(clueItem);
		GameManager.Instance?.ClueTracker.RegisterClue(woodenDollId);

		// InventoryUI_Complete 등록 (UI 표시용)
		var inventoryData = new InventoryItemData
		{
			itemId = woodenDollId,
			title = woodenDollName,
			description = woodenDollDialogue,
			itemType = ItemType.UsableItem,
			itemPrefab = woodenDollPrefab
		};
		var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
		inventoryUI?.AddItem(inventoryData);

		GameServices.UI?.ShowDialogue(speaker, woodenDollDialogue);
		Debug.Log($"[PuzzleSolveDoor] 목각인형 지급: {woodenDollName}");
	}
}