using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 1스테이지: 인형의 집 퍼즐
///
/// [기획서]
/// - 3D 드래그앤드롭 (UI 창 방식 아님, 스냅 없음)
/// - 맞는 슬롯에 드롭 → 인형 눈 뜸 (즉시 피드백)
/// - 틀린 슬롯 → 눈 안 뜸 + 오류 효과음
/// - 모든 슬롯 정답 → 자동 클리어
/// - 나갈 때(ESC) 리셋
/// - 실패 없음. 성공할 때까지 계속 시도 가능
///
/// [v3 변경사항]
/// 오답을 원위치로 튕겨내지 않고 그 자리에 남깁니다. 플레이어가 직접
/// 드래그해서 빼낼 수 있어, 이것저것 놓아보며 추리하는 흐름이 자연스러워집니다.
/// 정답 자리에 들어간 조각은 Lock()으로 고정되어 실수로 빠지지 않습니다.
/// 예전처럼 튕겨내려면 returnWrongItemHome을 체크하세요.
///
/// [v2 변경사항]
/// RegisterPickedItem() — Stage1_DollHousePickupClue가 주운 조각을 런타임 등록
/// </summary>
public class Stage1_DollHousePuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	[Header("드래그 아이템 목록")]
	[Tooltip("픽업 방식이면 비워두세요. 주운 것만 런타임으로 등록됩니다.")]
	[SerializeField] private List<PuzzleDraggableItem> dollItems = new List<PuzzleDraggableItem>();

	[Header("드롭존 목록 (인형의 집 슬롯)")]
	[Tooltip("PuzzleDropZone 오브젝트들. requiredItemId 반드시 설정.")]
	[SerializeField] private List<PuzzleDropZone> dollSlots = new List<PuzzleDropZone>();

	[Header("퍼즐 표면 높이")]
	[Tooltip("Horizontal 드래그 모드에서만 쓰입니다. " +
			 "ScreenPlane 모드면 이 값은 무시됩니다.")]
	[SerializeField] private float dollHouseSurfaceY = 0.5f;

	[Header("오답 처리")]
	[Tooltip("체크하면 틀린 자리에 놓았을 때 원래 자리로 되돌립니다. " +
			 "해제하면 그 자리에 남아 다시 드래그해서 뺄 수 있습니다.")]
	[SerializeField] private bool returnWrongItemHome = false;

	[Tooltip("체크하면 정답 자리에 들어간 조각을 고정해 다시 못 빼게 합니다.")]
	[SerializeField] private bool lockCorrectItems = true;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string startDialogue = "인형 장난감을 알맞은 자리에 놓아보자.";
	[TextArea(2, 4)][SerializeField] private string noItemDialogue = "놓을 만한 장난감이 없다. 거실을 더 둘러보자.";
	[TextArea(2, 4)][SerializeField] private string correctDialogue = "맞는 자리인 것 같아!";
	[TextArea(2, 4)][SerializeField] private string wrongDialogue = "...인형이 눈을 뜨지 않는다.";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "인형을 모두 제자리에 찾아줬다!";

	[Header("해결 연출")]
	[Tooltip("퍼즐 해결 시 잠깐 켜지는 조명.")]
	[SerializeField] private Light solveLight;
	[SerializeField] private float solveLightDuration = 1.2f;

	[Header("크리처")]
	[SerializeField] private GameObject creature;

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

	public void RegisterPickedItem(PuzzleDraggableItem item)
	{
		if (item == null || dollItems.Contains(item)) return;

		dollItems.Add(item);
		Debug.Log($"[Stage1DollHouse] 단서 등록: {item.itemId} ({dollItems.Count}/{dollSlots.Count})");
	}

	public int CollectedItemCount => dollItems.Count;
	public int RequiredItemCount => dollSlots.Count;

	// ── 퍼즐 시작 ─────────────────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		foreach (var item in dollItems)
		{
			if (item == null) continue;
			item.EnableDragging(_mainCamera, dollHouseSurfaceY);
		}

		_uiManager?.ShowDialogue(speaker,
			dollItems.Count == 0 ? noItemDialogue : startDialogue);
	}

	// ── 아이템이 슬롯에 놓일 때 ──────────────────────────────

	public void OnItemPlacedOnZone(PuzzleDropZone zone)
	{
		var placedItem = zone.GetPlacedItem();

		if (zone.IsCorrectlyFilled)
		{
			// 정답 — 고정해서 실수로 빠지지 않게
			if (lockCorrectItems) placedItem?.Lock();

			_uiManager?.ShowDialogue(speaker, correctDialogue);
			_audioManager?.PlaySFX("puzzle_correct");
		}
		else
		{
			// 오답 — 인형은 눈을 뜨지 않고, 조각은 그 자리에 남습니다.
			// 플레이어가 다시 드래그해서 빼낼 수 있습니다.
			if (returnWrongItemHome)
			{
				zone.RemoveItem();
				placedItem?.ResetToHomePosition();
			}

			_uiManager?.ShowDialogue(speaker, wrongDialogue);
			_audioManager?.PlaySFX("puzzle_wrong");
		}

		CheckSolution();
	}

	// ── 정답 판정 ────────────────────────────────────────────

	protected override bool IsSolutionCorrect()
	{
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

		foreach (var item in dollItems)
		{
			item?.Lock();
			item?.DisableDragging();
		}

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
		foreach (var item in dollItems)
		{
			item?.Lock();
			item?.DisableDragging();
		}
		if (creature != null) creature.SetActive(false);
	}

	// ── 퍼즐 나가기 ──────────────────────────────────────────

	public override void ExitPuzzle()
	{
		if (!isSolved)
		{
			foreach (var item in dollItems) item?.DisableDragging();
			ResetPuzzle();
		}

		base.ExitPuzzle();
	}

	private void ResetPuzzle()
	{
		foreach (var slot in dollSlots) slot?.RemoveItem();

		foreach (var item in dollItems)
		{
			item?.Unlock();
			item?.ResetToHomePosition();
		}

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