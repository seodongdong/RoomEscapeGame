using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 3스테이지: 인형 제작대 (조립 퍼즐)
///
/// [기획서]
/// - 출구 근처 인형 제작대에 상호작용
/// - 수집한 인형 조각들을 알맞은 위치에 드래그앤드롭하여 인형 조립
/// - 조립된 인형을 옆의 인형 크리처에게 건네주면
///   → 나무인형 획득 + 출구 열림 (효과음 재생)
///
/// [동작]
/// 퍼즐 진입 시 인벤토리에 있는 조각만 제작대 위에 나타납니다.
/// 조각이 모자라면 안내 대사만 나오고 조립할 수 없습니다.
/// 모든 조각을 제자리에 놓으면 조립 완료 → 완성 인형 오브젝트 활성화
/// → 옆의 Stage3_DollHandover(크리처)가 상호작용 가능해집니다.
///
/// [씬 설정]
/// 1. 제작대 오브젝트 근처 빈 오브젝트에 이 스크립트 부착
/// 2. PuzzleTrigger(제작대)의 Puzzle 슬롯에 연결
/// 3. parts: 조각마다 itemId / 드래그 오브젝트 / 드롭존을 한 줄씩 설정
///    - itemId는 Stage3_SlidingPuzzle의 rewardItems itemId와 정확히 같아야 합니다
///    - 드롭존의 requiredItemId도 같은 값으로 설정하세요
/// 4. assembledDollObject: 조립 완성 인형 (처음엔 비활성)
/// </summary>
public class Stage3_DollAssemblyTable : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	[System.Serializable]
	public class PartSlot
	{
		[Tooltip("인벤토리에 있어야 하는 조각 ID. 드롭존 requiredItemId와 동일해야 합니다.")]
		public string itemId = "doll_part_head";
		public string displayName = "인형 머리";
		[Tooltip("제작대 위에 나타날 드래그 가능한 3D 조각")]
		public PuzzleDraggableItem draggableItem;
		[Tooltip("이 조각이 들어갈 자리")]
		public PuzzleDropZone dropZone;
	}

	[Header("조각 목록")]
	[SerializeField]
	private List<PartSlot> parts = new List<PartSlot>()
	{
		new PartSlot { itemId = "doll_part_head",      displayName = "인형 머리" },
		new PartSlot { itemId = "doll_part_arm_left",  displayName = "인형 왼팔" },
		new PartSlot { itemId = "doll_part_arm_right", displayName = "인형 오른팔" },
		new PartSlot { itemId = "doll_part_leg_left",  displayName = "인형 왼다리" },
		new PartSlot { itemId = "doll_part_leg_right", displayName = "인형 오른다리" },
	};

	[Header("제작대 표면 Y 좌표")]
	[SerializeField] private float tableSurfaceY = 0.9f;

	[Header("완성 인형")]
	[Tooltip("조립 완료 시 활성화될 인형 오브젝트. 처음엔 비활성 상태로 두세요.")]
	[SerializeField] private GameObject assembledDollObject;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string startDialogue = "조각을 알맞은 자리에 맞춰보자.";
	[TextArea(2, 4)][SerializeField] private string missingPartsDialogue = "조각이 아직 부족하다.";
	[TextArea(2, 4)][SerializeField] private string correctDialogue = "여기가 맞는 것 같아.";
	[TextArea(2, 4)][SerializeField] private string wrongDialogue = "...여기가 아니야.";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "인형이 완성됐다. 저 인형에게 건네줘야 할까?";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "나중에 다시 맞춰보자.";

	// ── 런타임 ────────────────────────────────────────────────
	private bool _hasAllParts = false;

	public bool IsAssembled => isSolved;

	// ── 초기화 ────────────────────────────────────────────────

	protected override void Awake()
	{
		base.Awake();

		foreach (var part in parts)
		{
			part.dropZone?.Initialize(this);

			// 조각은 퍼즐에 들어가기 전까지 숨겨둡니다
			if (part.draggableItem != null)
				part.draggableItem.gameObject.SetActive(false);
		}

		if (assembledDollObject != null)
			assembledDollObject.SetActive(false);
	}

	// ── 퍼즐 시작 ─────────────────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		_hasAllParts = CheckInventoryForAllParts();

		foreach (var part in parts)
		{
			if (part.draggableItem == null) continue;

			bool owned = HasPart(part.itemId);
			part.draggableItem.gameObject.SetActive(owned);

			if (owned)
			{
				part.draggableItem.itemId = part.itemId;
				part.draggableItem.EnableDragging(_mainCamera, tableSurfaceY);
			}
		}

		GameServices.UI?.ShowDialogue(speaker,
			_hasAllParts ? startDialogue : missingPartsDialogue);
	}

	private bool CheckInventoryForAllParts()
	{
		foreach (var part in parts)
			if (!HasPart(part.itemId)) return false;
		return true;
	}

	private bool HasPart(string itemId)
	{
		var player = GameServices.Player;
		if (player == null || string.IsNullOrEmpty(itemId)) return false;
		return player.Inventory.HasItem(itemId);
	}

	// ── IDropZonePuzzle ───────────────────────────────────────

	public void OnItemPlacedOnZone(PuzzleDropZone zone)
	{
		if (zone.IsCorrectlyFilled)
		{
			GameServices.UI?.ShowDialogue(speaker, correctDialogue);
			GameServices.Audio?.PlaySFX("puzzle_correct");
		}
		else
		{
			var placed = zone.GetPlacedItem();
			zone.RemoveItem();
			placed?.ResetToHomePosition();

			GameServices.UI?.ShowDialogue(speaker, wrongDialogue);
			GameServices.Audio?.PlaySFX("puzzle_wrong");
		}

		CheckSolution();
	}

	// ── 정답 판정 ────────────────────────────────────────────

	protected override bool IsSolutionCorrect()
	{
		if (parts == null || parts.Count == 0) return false;
		if (!_hasAllParts) return false;

		foreach (var part in parts)
		{
			if (part.dropZone == null) return false;
			if (!part.dropZone.IsCorrectlyFilled) return false;
		}
		return true;
	}

	// ── 조립 완료 ────────────────────────────────────────────

	protected override void SolvePuzzle()
	{
		foreach (var part in parts)
			part.draggableItem?.DisableDragging();

		StartCoroutine(ShowAssembledDoll());

		GameServices.UI?.ShowDialogue(speaker, solveDialogue);
		GameServices.Audio?.PlaySFX("puzzle_solved");

		base.SolvePuzzle();
	}

	private IEnumerator ShowAssembledDoll()
	{
		yield return new WaitForSecondsRealtime(0.4f);

		// 조각 숨기고 완성 인형 표시
		foreach (var part in parts)
			if (part.draggableItem != null) part.draggableItem.gameObject.SetActive(false);

		if (assembledDollObject != null)
			assembledDollObject.SetActive(true);
	}

	protected override void OnLoadStateSolved()
	{
		foreach (var part in parts)
		{
			part.draggableItem?.DisableDragging();
			if (part.draggableItem != null) part.draggableItem.gameObject.SetActive(false);
		}

		if (assembledDollObject != null)
			assembledDollObject.SetActive(true);
	}

	// ── 퍼즐 나가기 (기획서: 풀다가 나가면 리셋) ─────────────

	public override void ExitPuzzle()
	{
		if (!isSolved)
		{
			foreach (var part in parts)
			{
				part.dropZone?.RemoveItem();
				part.draggableItem?.DisableDragging();
				part.draggableItem?.ResetToHomePosition();
				if (part.draggableItem != null) part.draggableItem.gameObject.SetActive(false);
			}

			GameServices.UI?.ShowDialogue(speaker, exitDialogue);
			Debug.Log("[DollAssembly] 미완성 상태로 나감 — 리셋");
		}

		base.ExitPuzzle();
	}
}