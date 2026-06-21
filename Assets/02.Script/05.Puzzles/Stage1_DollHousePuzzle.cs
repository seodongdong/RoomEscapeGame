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
///
/// [수정]
/// - exitButton 제거 → ESC로 나가기 (CameraPuzzleBase + UILayerManager 처리)
/// - puzzleUI 제거 → CameraPuzzleBase에서 이미 제거됨
/// - IsSolutionCorrect: 슬롯 없으면 false (즉시 클리어 방지)
/// - 오답 시 GetPlacedItem()으로 아이템 명시적 원위치
/// </summary>
public class Stage1_DollHousePuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	[Header("드래그 아이템 목록")]
	[Tooltip("씬에 배치된 PuzzleDraggableItem 오브젝트들 연결")]
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
	[TextArea(2, 4)][SerializeField] private string correctDialogue = "맞는 자리인 것 같아!";
	[TextArea(2, 4)][SerializeField] private string wrongDialogue = "...여기가 아닌 것 같다.";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "인형을 모두 제자리에 찾아줬다!";

	[Header("크리처")]
	[SerializeField] private GameObject creature;

	// ── 캐싱 ─────────────────────────────────────────────────
	private UIManager _uiManager;
	private AudioManager _audioManager;

	// ── 초기화 ────────────────────────────────────────────────
	protected override void Awake()
	{
		base.Awake();

		// 드롭존에 이 퍼즐 등록
		foreach (var slot in dollSlots)
			if (slot != null) slot.Initialize(this);
	}

	protected override void Start()
	{
		base.Start();
		_uiManager = GameServices.UI;
		_audioManager = GameServices.Audio;
	}

	// ── 퍼즐 시작 콜백 ───────────────────────────────────────
	protected override void OnPuzzleStarted()
	{
		// 드래그 활성화
		foreach (var item in dollItems)
			if (item != null) item.EnableDragging(_mainCamera, dollHouseSurfaceY);

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
			// 오답: 슬롯 초기화 + 아이템 원위치
			var placedItem = zone.GetPlacedItem();
			zone.RemoveItem();                  // emptySprite 복원
			placedItem?.ResetToHomePosition();  // 아이템 원위치

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
		// 모든 슬롯 bigSmile 표정
		foreach (var slot in dollSlots) slot?.SetBigSmileExpression();

		// 드래그 비활성화
		foreach (var item in dollItems) item?.DisableDragging();

		_uiManager?.ShowDialogue(speaker, solveDialogue);
		_audioManager?.PlaySFX("door_unlock");

		// 크리처 비활성화
		if (creature != null) creature.SetActive(false);

		// base: isSolved=true, UILayerManager.Pop, OnPuzzleSolved 이벤트, ExitPuzzle
		base.SolvePuzzle();
	}

	// ── 퍼즐 나가기 (ESC 또는 해결 시) ──────────────────────
	public override void ExitPuzzle()
	{
		if (!isSolved)
		{
			// 미해결 상태로 나가면 리셋
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