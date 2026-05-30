using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 2스테이지 제단 사탕 퍼즐 (v4)
///
/// [기획서 기준 동작]
/// 1. 어느 방석에나 사탕을 올릴 수 있음
/// 2. 사탕을 올리는 순간 그 자리 사진이 웃는 표정으로 바뀜
/// 3. 5개를 다 올렸을 때:
///    - 오답 → 잠깐 대기 후 자동 리셋 (사탕 전부 원위치)
///    - 정답 → 마지막 자리만 활짝 웃는 표정, 퍼즐 완료
/// </summary>
public class Stage2_AltarCandyPuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	[Header("월드 스페이스 퍼즐 오브젝트")]
	[SerializeField] private List<PuzzleDraggableItem> candyItems = new List<PuzzleDraggableItem>();
	[SerializeField] private List<PuzzleDropZone> dropZones = new List<PuzzleDropZone>();

	[Tooltip("관 표면의 Y 좌표")]
	[SerializeField] private float coffinSurfaceY = 0.5f;

	[Header("크리처 연동")]
	[SerializeField] private Stage2_ShadowCreature shadowCreature;

	[Header("오답 리셋 설정")]
	[Tooltip("5개 다 놓은 뒤 오답 확인 전 대기 시간 (초). 웃는 표정을 잠깐 보여줍니다.")]
	[SerializeField] private float wrongAnswerDelay = 2.0f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "...다음 방으로 갈 수 있을 것 같다.";
	[TextArea(2, 4)][SerializeField] private string wrongDialogue = "...아닌 것 같다.";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "나중에 다시 와야겠다.";

	private PuzzleDropZone _lastPlacedZone; // 마지막으로 사탕을 받은 존
	private bool _isCheckingAnswer = false; // 오답 판정 중 중복 체크 방지

	protected override void Awake()
	{
		base.Awake();
		foreach (var zone in dropZones)
			if (zone != null) zone.Initialize(this);
	}

	// ── IDropZonePuzzle ───────────────────────────────────────

	/// <summary>
	/// 어느 존에든 사탕이 놓일 때마다 호출됩니다.
	/// 5개가 다 채워지면 정답 체크를 시작합니다.
	/// </summary>
	public void OnItemPlacedOnZone(PuzzleDropZone zone)
	{
		if (_isCheckingAnswer) return;

		_lastPlacedZone = zone;
		int placedCount = CountPlacedCandies();

		Debug.Log($"[AltarPuzzle] {placedCount}/5 배치됨");

		if (placedCount >= candyItems.Count)
		{
			// 5개 다 놓임 → 정답 체크 시작
			StartCoroutine(CheckAnswerCoroutine());
		}
	}

	// ── 정답 체크 코루틴 ─────────────────────────────────────

	private IEnumerator CheckAnswerCoroutine()
	{
		_isCheckingAnswer = true;

		// 잠깐 대기 (웃는 표정 감상)
		yield return new WaitForSecondsRealtime(wrongAnswerDelay);

		if (IsSolutionCorrect())
		{
			// 정답: 5개 사진 전부 활짝 웃는 표정으로 변경
			foreach (var zone in dropZones)
				if (zone != null && zone.IsOccupied)
					zone.SetBigSmileExpression();

			_isCheckingAnswer = false;
			SolvePuzzle();
		}
		else
		{
			// 오답: 대사 출력 후 자동 리셋
			FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, wrongDialogue);
			yield return new WaitForSecondsRealtime(1f);
			ResetPuzzle();
			_isCheckingAnswer = false;
		}
	}

	// ── 정답 판정 ────────────────────────────────────────────

	protected override bool IsSolutionCorrect()
	{
		// 정답 존(requiredItemId 또는 requiredColor가 설정된 존)이
		// 모두 올바른 아이템으로 채워졌는지 확인
		foreach (var zone in dropZones)
		{
			if (zone == null) continue;

			bool isRequiredZone = !string.IsNullOrEmpty(zone.requiredItemId) ||
								  (zone.requiredColor != Color.white && zone.requiredColor != Color.clear);

			if (isRequiredZone && !zone.IsCorrectlyFilled)
				return false;
		}
		return true;
	}

	protected override void SolvePuzzle()
	{
		foreach (var candy in candyItems)
			if (candy != null) candy.DisableDragging();

		shadowCreature?.MoveToFinalPosition();
		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, solveDialogue);
		base.SolvePuzzle();
	}

	// ── 퍼즐 나가기 ──────────────────────────────────────────

	public override void ExitPuzzle()
	{
		foreach (var candy in candyItems)
			if (candy != null) candy.DisableDragging();

		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, exitDialogue);
		base.ExitPuzzle();
	}

	public void ExitPuzzlePreserveState()
	{
		foreach (var candy in candyItems)
			if (candy != null) candy.DisableDragging();
		base.ExitPuzzle();
	}

	// ── 퍼즐 시작(재진입) ────────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();
		Camera cam = Camera.main;
		foreach (var candy in candyItems)
			if (candy != null) candy.EnableDragging(cam, coffinSurfaceY);
	}

	// ── 리셋 ─────────────────────────────────────────────────

	private void ResetPuzzle()
	{
		_lastPlacedZone = null;

		foreach (var zone in dropZones)
			if (zone != null) zone.RemoveItem();

		foreach (var candy in candyItems)
			if (candy != null) candy.ResetToHomePosition();

		Debug.Log("[AltarPuzzle] 오답 → 자동 리셋");
	}

	// ── 헬퍼 ─────────────────────────────────────────────────

	private int CountPlacedCandies()
	{
		int count = 0;
		foreach (var zone in dropZones)
			if (zone != null && zone.IsOccupied) count++;
		return count;
	}
}