using UnityEngine;
using System.Collections;

public class PuzzleSolveDoor : MonoBehaviour, IInteractable
{
	[Header("퍼즐 연결")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("문 설정")]
	[SerializeField] private Animator doorAnimator;
	[SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0);
	[SerializeField] private float openDuration = 1f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "퍼즐을 풀어야 열 수 있다...";
	[TextArea(2, 5)]
	[SerializeField] private string openDialogue = "문이 열렸다!";

	[Header("나무인형 보상")]
	[SerializeField] private string woodenDollId = "";       // 비우면 지급 안 함
	[SerializeField] private string woodenDollName = "나무인형";
	[TextArea(1, 2)]
	[SerializeField] private string woodenDollDialogue = "나무인형을 획득했다!";
	[SerializeField] private GameObject woodenDollPrefab;

	private IPuzzle _puzzle;
	private bool _isOpen = false;
	private bool _dollGiven = false;
	private Vector3 _closedPosition;

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;
		_closedPosition = transform.position;
	}

	private void Start()
	{
		if (_puzzle != null)
			_puzzle.OnPuzzleSolved += OnPuzzleSolved;
	}

	private void OnDestroy()
	{
		if (_puzzle != null)
			_puzzle.OnPuzzleSolved -= OnPuzzleSolved;
	}

	// ── IInteractable ──────────────────────────
	public string InteractionPrompt => _isOpen ? "[F] 문 열기" : "퍼즐을 먼저 풀어야 한다...";

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();
		if (!_isOpen)
			uiManager?.ShowDialogue(speaker, lockedDialogue);
		else
			OpenDoor();
	}

	// ── 퍼즐 해결 시 자동 호출 ─────────────────
	private void OnPuzzleSolved()
	{
		_isOpen = true;
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, openDialogue);
		StartCoroutine(OpenDoorCoroutine());
	}

	private void OpenDoor()
	{
		StartCoroutine(OpenDoorCoroutine());
	}

	private IEnumerator OpenDoorCoroutine()
	{
		if (doorAnimator != null)
		{
			doorAnimator.SetTrigger("Open");
		}
		else
		{
			// 애니메이터 없을 때 위로 이동
			Vector3 targetPos = _closedPosition + openOffset;
			float elapsed = 0f;
			while (elapsed < openDuration)
			{
				elapsed += Time.deltaTime;
				transform.position = Vector3.Lerp(_closedPosition, targetPos, elapsed / openDuration);
				yield return null;
			}
			transform.position = targetPos;
		}
	}

	// ── 플레이어 통과 시 나무인형 지급 ──────────
	private void OnTriggerEnter(Collider other)
	{
		if (_dollGiven || string.IsNullOrEmpty(woodenDollId)) return;
		if (!other.CompareTag("Player")) return;
		if (!_isOpen) return;

		var player = other.GetComponent<Player>();
		if (player == null) return;
		if (player.Inventory.HasItem(woodenDollId)) return;

		_dollGiven = true;

		// PlayerInventory 등록
		player.Inventory.AddItem(new ClueItem(woodenDollId, woodenDollName, "퍼즐을 풀어 획득한 나무인형"));
		GameManager.Instance.ClueTracker.RegisterClue(woodenDollId);

		// InventoryUI 등록 — 이번엔 itemPrefab 포함
		var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
		inventoryUI?.AddItem(new InventoryItemData
		{
			itemId = woodenDollId,
			title = woodenDollName,
			date = "",
			itemType = ItemType.UsableItem,
			description = "퍼즐을 풀어 획득한 나무인형",
			pages = null,
			itemPrefab = woodenDollPrefab  // ← 프리팹 연결
		});

		// 대사 출력
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, woodenDollDialogue);
	}
}
