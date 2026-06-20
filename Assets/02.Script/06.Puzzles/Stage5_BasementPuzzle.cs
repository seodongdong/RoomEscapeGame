using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 5스테이지 지하실 퍼즐: 목각인형 장식장 배열 - 월드 스페이스 드래그앤드랍 버전
///
/// [기획서 기준 동작]
/// 1~4스테이지에서 수집한 목각인형 4개를 장식장의 올바른 슬롯에 배치합니다.
/// 퍼즐 진입 시, 플레이어 인벤토리에 있는 목각인형들을 3D 오브젝트로 씬에 소환합니다.
/// 모두 올바르게 배치하면 상자 열쇠가 등장합니다.
///
/// [Stage 2/4와의 차이점]
/// 아이템이 인벤토리에서 오기 때문에 퍼즐 시작 시 소환(Instantiate)이 필요합니다.
/// 모든 인형을 모아오지 않은 상태에서도 일부만 가지고 진입할 수 있습니다.
/// (가진 인형만 소환됩니다. 정답 슬롯 수 == 소환된 인형 수여야 퍼즐 완료 가능.)
///
/// [버그 수정]
/// - IsSolutionCorrect()에서 slot.IsCorrect 체크 시 filledCount를 증가시키는 코드가
///   주석 처리되어 있어 filledCount가 항상 0으로 남아, 인형을 모두 올바르게 배치해도
///   퍼즐이 절대 클리어되지 않던 문제 수정. (정답 조건 자체는 변경 없음:
///   "소환된 인형 수 == 올바르게 채워진 슬롯 수")
///
/// [씬 설정]
/// 1. shelfSlots: 장식장 위 빈 오브젝트들 (PuzzleDropZone + requiredItemId 설정)
///    예: 1번 슬롯 → requiredItemId = "wooden_doll_stage1"
/// 2. woodenDolls: 각 목각인형의 inventoryItemId, prefab, spawnPoint 설정
///    prefab에는 PuzzleDraggableItem 컴포넌트가 붙어있어야 합니다.
/// 3. keyObject: 퍼즐 완료 시 나타날 열쇠 오브젝트 연결
/// </summary>
public class Stage5_BasementPuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	// ── 목각인형 데이터 ──────────────────────────
	[System.Serializable]
	public class WoodenDollEntry
	{
		[Tooltip("플레이어 인벤토리에서 확인할 아이템 ID.\n예: wooden_doll_stage1")]
		public string inventoryItemId;

		[Tooltip("퍼즐 시작 시 소환할 3D 프리팹.\nPuzzleDraggableItem 컴포넌트가 있어야 합니다.")]
		public GameObject prefab;

		[Tooltip("소환될 위치 (장식장 앞 테이블이나 바닥 위).")]
		public Transform spawnPoint;
	}

	[Header("목각인형 목록 (스테이지 1~4 순서)")]
	[SerializeField] private List<WoodenDollEntry> woodenDolls = new List<WoodenDollEntry>();

	[Header("장식장 슬롯들")]
	[Tooltip("PuzzleDropZone 컴포넌트 + requiredItemId가 설정된 슬롯 오브젝트들.")]
	[SerializeField] private List<PuzzleDropZone> shelfSlots = new List<PuzzleDropZone>();

	[Header("퍼즐 완료 보상")]
	[Tooltip("퍼즐 완료 시 나타날 열쇠 오브젝트.")]
	[SerializeField] private GameObject keyObject;

	[Tooltip("플레이어 인벤토리에 추가될 열쇠 아이템 ID.")]
	[SerializeField] private string keyItemId = "basement_key";
	[SerializeField] private string keyItemName = "녹슨 열쇠";
	[SerializeField] private string keyItemDesc = "오래된 상자를 열 수 있을 것 같다.";

	[Tooltip("목각인형을 드래그할 표면의 Y 좌표. 장식장 선반의 Y값을 넣으세요.")]
	[SerializeField] private float shelfSurfaceY = 0.5f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "뭔가 열리는 소리가 들린다.";
	[TextArea(2, 4)][SerializeField] private string notEnoughDollsDialogue = "아직 인형들이 부족하다...";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "다시 나가서 더 찾아봐야겠다.";

	// 퍼즐 시작 시 소환된 인형 오브젝트들 (나갈 때 정리 및 재입장 시 재소환용)
	private readonly List<PuzzleDraggableItem> _spawnedDolls = new List<PuzzleDraggableItem>();

	// ────────────────────────────────────────────
	// 초기화
	// ────────────────────────────────────────────

	protected override void Awake()
	{
		base.Awake();
		foreach (var slot in shelfSlots)
			if (slot != null) slot.Initialize(this);

		if (keyObject != null) keyObject.SetActive(false);
	}

	// ────────────────────────────────────────────
	// IDropZonePuzzle 구현
	// ────────────────────────────────────────────

	public void OnItemPlacedOnZone(PuzzleDropZone zone)
	{
		Debug.Log($"[BasementPuzzle] 인형 배치: {zone.requiredItemId}");
		CheckSolution();
	}

	// ────────────────────────────────────────────
	// 퍼즐 시작
	// ────────────────────────────────────────────

	public override void StartPuzzle()
	{
		if (isSolved) return;

		// 인형이 하나도 없으면 안내 후 차단
		var player = FindAnyObjectByType<Player>();
		if (player != null && !HasAnyDoll(player))
		{
			FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, notEnoughDollsDialogue);
			return;
		}

		base.StartPuzzle(); // 카메라 이동 시작
	}

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();
		SpawnCollectedDolls();
	}

	private bool HasAnyDoll(Player player)
	{
		foreach (var entry in woodenDolls)
			if (!string.IsNullOrEmpty(entry.inventoryItemId) &&
				player.Inventory.HasItem(entry.inventoryItemId)) return true;
		return false;
	}

	/// <summary>
	/// 인벤토리에 있는 목각인형들만 씬에 소환합니다.
	/// 없는 인형은 소환하지 않습니다 (그 슬롯은 못 채움).
	/// </summary>
	private void SpawnCollectedDolls()
	{
		_spawnedDolls.Clear();
		var player = FindAnyObjectByType<Player>();
		if (player == null) return;

		Camera cam = Camera.main;

		foreach (var entry in woodenDolls)
		{
			if (string.IsNullOrEmpty(entry.inventoryItemId)) continue;
			if (!player.Inventory.HasItem(entry.inventoryItemId)) continue;
			if (entry.prefab == null || entry.spawnPoint == null) continue;

			// 소환
			GameObject dollObj = Instantiate(entry.prefab, entry.spawnPoint.position, Quaternion.identity);
			var draggable = dollObj.GetComponent<PuzzleDraggableItem>();

			if (draggable != null)
			{
				draggable.itemId = entry.inventoryItemId; // ID 설정 (슬롯 매칭용)
				draggable.EnableDragging(cam, shelfSurfaceY);
				_spawnedDolls.Add(draggable);
			}
		}

		Debug.Log($"[BasementPuzzle] {_spawnedDolls.Count}개 목각인형 소환됨.");
	}

	// ────────────────────────────────────────────
	// 정답 판정
	// ────────────────────────────────────────────

	/// <summary>
	/// 소환된 목각인형 수와, 올바르게 채워진 슬롯 수가 일치해야 완료됩니다.
	/// [버그 수정] filledCount++ 주석 해제 — 실제 카운트가 동작하도록 복원.
	/// </summary>
	protected override bool IsSolutionCorrect()
	{
		if (shelfSlots.Count == 0) return false;

		// 슬롯이 여러 개인데, 소환된 인형이 부족하면 일부 슬롯은 영원히 못 채움
		// → 소환된 인형 수 == 채워진 슬롯 수 이어야 완료
		int filledCount = 0;
		foreach (var slot in shelfSlots)
		{
			if (slot == null) continue;
			if (slot.IsCorrect) filledCount++;
		}
		return filledCount > 0 && filledCount == _spawnedDolls.Count;
	}

	protected override void SolvePuzzle()
	{
		// 드래그 비활성화
		foreach (var doll in _spawnedDolls)
			if (doll != null) doll.DisableDragging();

		// 열쇠 등장
		if (keyObject != null) keyObject.SetActive(true);

		// 열쇠 아이템 지급
		var player = FindAnyObjectByType<Player>();
		if (player != null)
		{
			var key = new ClueItem(keyItemId, keyItemName, keyItemDesc);
			player.Inventory.AddItem(key);
		}

		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, solveDialogue);
		base.SolvePuzzle();
	}

	// ────────────────────────────────────────────
	// 나가기 (소환된 인형 정리)
	// ────────────────────────────────────────────

	public override void ExitPuzzle()
	{
		// 슬롯 초기화
		foreach (var slot in shelfSlots)
			if (slot != null) slot.RemoveItem();

		// 소환된 인형 제거 (재입장 시 다시 소환됨)
		foreach (var doll in _spawnedDolls)
			if (doll != null) Destroy(doll.gameObject);
		_spawnedDolls.Clear();

		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, exitDialogue);
		base.ExitPuzzle();
	}
}