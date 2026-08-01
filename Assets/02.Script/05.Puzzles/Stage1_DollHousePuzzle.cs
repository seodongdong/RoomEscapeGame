using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 1스테이지: 인형의 집 퍼즐
///
/// [기획서]
/// - 3D 드래그앤드롭 (UI 창/패널 방식 아님, 스냅 없음)
/// - 맞는 슬롯에 드롭 → 인형 눈 뜸 (즉시 피드백)
/// - 틀린 슬롯 → 눈 안 뜸 + 오류 효과음 + 아이템 원위치
/// - 모든 슬롯 정답 → 자동 클리어
/// - 나갈 때(ESC) 리셋
/// - 실패 없음. 성공할 때까지 계속 시도 가능
///
/// [이번 수정]
/// 1. RegisterPickedItem() 추가
///    Stage1_DollHousePickupClue가 거실에서 프랍을 주울 때마다
///    런타임으로 드래그 아이템을 등록합니다. Inspector에 미리 넣어둘
///    필요 없이, "주운 것만" 퍼즐에 나타납니다.
/// 2. IsSolutionCorrect가 "모든 슬롯이 채워졌는지"까지 확인
///    → 아직 안 주운 단서가 있으면 클리어되지 않습니다.
/// 3. 아직 아무것도 줍지 않은 상태로 진입했을 때의 안내 대사 추가
/// </summary>
public class Stage1_DollHousePuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	[Header("드래그 아이템 목록")]
	[Tooltip("씬에 미리 배치해둘 경우 여기에 연결. " +
			 "Stage1_DollHousePickupClue로 줍는 방식이면 비워둬도 됩니다.")]
	[SerializeField] private List<PuzzleDraggableItem> dollItems = new List<PuzzleDraggableItem>();

	[Header("드롭존 목록 (인형의 집 슬롯)")]
	[Tooltip("PuzzleDropZone 오브젝트들 연결. requiredItemId 반드시 설정")]
	[SerializeField] private List<PuzzleDropZone> dollSlots = new List<PuzzleDropZone>();

	[Header("퍼즐 표면 높이")]
	[Tooltip("드래그 평면 Y좌표 — 인형의 집 선반/바닥 높이")]
	[SerializeField] private float dollHouseSurfaceY = 0.5f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string startDialogue = "인형 장난감을 알맞은 자리에 놓아보자.";
	[TextArea(2, 4)][SerializeField] private string noItemDialogue = "놓을 만한 장난감이 없다. 거실을 더 둘러보자.";
	[TextArea(2, 4)][SerializeField] private string correctDialogue = "맞는 자리인 것 같아!";
	[TextArea(2, 4)][SerializeField] private string wrongDialogue = "...여기가 아닌 것 같다.";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "인형을 모두 제자리에 찾아줬다!";

	[Header("해결 연출")]
	[Tooltip("퍼즐 해결 시 잠깐 켜지는 조명. (기획서: 짧은 조명 연출 + 효과음)")]
	[SerializeField] private Light solveLight;
	[SerializeField] private float solveLightDuration = 1.2f;

	[Header("크리처")]
	[SerializeField] private GameObject creature;

	// ── 캐싱 ─────────────────────────────────────────────────
	private UIManager _uiManager;
	private AudioManager _audioManager;

	// ── 초기화 ────────────────────────────────────────────────
	protected override void Awake()
	{
		base.Awake();

		foreach (var slot in dollSlots)
			if (slot != null) slot.Initialize(this);

		if (solveLight != null) solveLight.enabled = false;
	}

	protected override void Start()
	{
		base.Start();
		_uiManager = GameServices.UI;
		_audioManager = GameServices.Audio;
	}

	// ── 수집 단서 런타임 등록 ────────────────────────────────

	/// <summary>
	/// Stage1_DollHousePickupClue가 거실에서 프랍을 주울 때 호출합니다.
	/// 중복 등록은 무시합니다.
	/// </summary>
	public void RegisterPickedItem(PuzzleDraggableItem item)
	{
		if (item == null) return;
		if (dollItems.Contains(item)) return;

		dollItems.Add(item);
		Debug.Log($"[Stage1DollHouse] 단서 등록: {item.itemId} ({dollItems.Count}/{dollSlots.Count})");
	}

	/// <summary>현재까지 수집한 단서 개수 (Flow/디버그용)</summary>
	public int CollectedItemCount => dollItems.Count;

	/// <summary>이 퍼즐이 요구하는 총 단서 개수</summary>
	public int RequiredItemCount => dollSlots.Count;

	// ── 퍼즐 시작 콜백 ───────────────────────────────────────
	protected override void OnPuzzleStarted()
	{
		foreach (var item in dollItems)
			if (item != null) item.EnableDragging(_mainCamera, dollHouseSurfaceY);

		if (dollItems.Count == 0)
			_uiManager?.ShowDialogue(speaker, noItemDialogue);
		else
			_uiManager?.ShowDialogue(speaker, startDialogue);
	}

	// ── 아이템이 슬롯에 놓일 때 ──────────────────────────────
	public void OnItemPlacedOnZone(PuzzleDropZone zone)
	{
		if (zone.IsCorrectlyFilled)
		{
			// 정답: smileSprite는 PuzzleDropZone.TryAcceptItem에서 이미 처리됨
			_uiManager?.ShowDialogue(speaker, correctDialogue);
			_audioManager?.PlaySFX("puzzle_correct");
		}
		else
		{
			// 오답: 슬롯 초기화 + 아이템 원위치 (실패 없음, 계속 시도 가능)
			var placedItem = zone.GetPlacedItem();
			zone.RemoveItem();
			placedItem?.ResetToHomePosition();

			_uiManager?.ShowDialogue(speaker, wrongDialogue);
			_audioManager?.PlaySFX("puzzle_wrong");
		}

		CheckSolution();
	}

	// ── 정답 판정 ────────────────────────────────────────────
	protected override bool IsSolutionCorrect()
	{
		// 슬롯이 없으면 false (즉시 클리어 방지)
		if (dollSlots == null || dollSlots.Count == 0) return false;

		// 아직 안 주운 단서가 있으면 클리어 불가
		if (dollItems.Count < dollSlots.Count) return false;

		foreach (var slot in dollSlots)
		{
			if (slot == null) continue;
			if (!slot.IsCorrectlyFilled) return false;
		}
		return true;
	}

	// ── 퍼즐 해결 ────────────────────────────────────────────
	protected override void SolvePuzzle()
	{
		foreach (var slot in dollSlots) slot?.SetBigSmileExpression();
		foreach (var item in dollItems) item?.DisableDragging();

		_uiManager?.ShowDialogue(speaker, solveDialogue);
		_audioManager?.PlaySFX("puzzle_solved");
		_audioManager?.PlaySFX("door_unlock");

		if (solveLight != null) StartCoroutine(FlashSolveLight());

		if (creature != null) creature.SetActive(false);

		base.SolvePuzzle();
	}

	private System.Collections.IEnumerator FlashSolveLight()
	{
		solveLight.enabled = true;
		yield return new WaitForSeconds(solveLightDuration);
		solveLight.enabled = false;
	}

	// ── 저장 복원 ────────────────────────────────────────────
	protected override void OnLoadStateSolved()
	{
		foreach (var slot in dollSlots) slot?.SetBigSmileExpression();
		foreach (var item in dollItems) item?.DisableDragging();
		if (creature != null) creature.SetActive(false);
	}

	// ── 퍼즐 나가기 (ESC 또는 해결 시) ──────────────────────
	public override void ExitPuzzle()
	{
		if (!isSolved)
		{
			foreach (var item in dollItems) item?.DisableDragging();
			ResetPuzzle();
		}

		base.ExitPuzzle();
	}

	// ── 리셋 ─────────────────────────────────────────────────
	private void ResetPuzzle()
	{
		foreach (var slot in dollSlots) slot?.RemoveItem();
		foreach (var item in dollItems) item?.ResetToHomePosition();
		Debug.Log("[Stage1DollHouse] 퍼즐 리셋");
	}

	// ── 기즈모 ────────────────────────────────────────────────
	private void OnDrawGizmos()
	{
		if (dollSlots == null) return;
		Gizmos.color = Color.cyan;
		foreach (var slot in dollSlots)
			if (slot != null) Gizmos.DrawWireSphere(slot.transform.position, 0.2f);
	}
}