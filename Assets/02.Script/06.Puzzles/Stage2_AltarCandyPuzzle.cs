using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 2스테이지 제단 사탕 퍼즐 - 월드 스페이스 드래그앤드랍 버전 (v2)
///
/// [v2 변경사항]
/// PuzzleDropZone.IDropZonePuzzle 인터페이스 구현 추가.
/// Initialize() 호출 시 this(IDropZonePuzzle)를 전달합니다.
/// </summary>
public class Stage2_AltarCandyPuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	[Header("월드 스페이스 퍼즐 오브젝트")]
	[SerializeField] private List<PuzzleDraggableItem> candyItems = new List<PuzzleDraggableItem>();
	[SerializeField] private List<PuzzleDropZone> dropZones = new List<PuzzleDropZone>();

	[Tooltip("관 표면의 Y 좌표. 씬에서 관 오브젝트의 Transform Y값을 넣으면 됩니다.")]
	[SerializeField] private float coffinSurfaceY = 0.5f;

	[Header("크리처 연동")]
	[SerializeField] private Stage2_ShadowCreature shadowCreature;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "...다음 방으로 갈 수 있을 것 같다.";
	[TextArea(2, 4)][SerializeField] private string exitPuzzleDialogue = "나중에 다시 와야겠다.";

	protected override void Awake()
	{
		base.Awake();
		// IDropZonePuzzle로 자신을 등록 (v2: 인터페이스 사용)
		foreach (var zone in dropZones)
			if (zone != null) zone.Initialize(this);
	}

	// ── IDropZonePuzzle 구현 ──
	public void OnItemPlacedOnZone(PuzzleDropZone zone)
	{
		Debug.Log($"[AltarPuzzle] 슬롯 {zone.slotIndex} 배치 완료");
		CheckSolution();
	}

	// ── 퍼즐 시작 ──
	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();
		Camera cam = Camera.main;
		foreach (var candy in candyItems)
			if (candy != null) candy.EnableDragging(cam, coffinSurfaceY);
	}

	// ── 정답 체크 ──
	protected override bool IsSolutionCorrect()
	{
		int correctCount = 0;
		int requiredCount = 0;

		foreach (var zone in dropZones)
		{
			if (zone == null) continue;
			// requiredColor가 흰색/기본이 아닌 슬롯 = 정답이 있는 슬롯
			if (zone.requiredColor != Color.white && zone.requiredColor != Color.clear)
			{
				requiredCount++;
				if (zone.IsCorrect) correctCount++;
			}
		}
		return requiredCount > 0 && correctCount >= requiredCount;
	}

	protected override void SolvePuzzle()
	{
		foreach (var zone in dropZones)
			if (zone != null && zone.IsCorrect) zone.SetBigSmileExpression();

		foreach (var candy in candyItems)
			if (candy != null) candy.DisableDragging();

		shadowCreature?.MoveToFinalPosition();

		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, solveDialogue);
		base.SolvePuzzle();
	}

	// ── 퍼즐 나가기 (리셋) ──
	public override void ExitPuzzle()
	{
		ResetPuzzle();
		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, exitPuzzleDialogue);
		base.ExitPuzzle();
	}

	private void ResetPuzzle()
	{
		foreach (var zone in dropZones) if (zone != null) zone.RemoveItem();
		foreach (var candy in candyItems) if (candy != null) candy.ResetToOriginalPosition();
		Debug.Log("[AltarPuzzle] 퍼즐 리셋됨");
	}
}