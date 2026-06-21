using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 4스테이지 장난감 요리 퍼즐 - 월드 스페이스 드래그앤드랍 버전
///
/// [기획서 기준 동작]
/// 조리대 위에 장난감 재료들이 놓여있습니다.
/// 플레이어가 F키로 조리대에 다가가면 카메라가 위에서 내려다보는 시점으로 이동합니다.
/// 레시피 메모를 참고해서 각 재료를 접시의 올바른 자리에 드래그하면 됩니다.
/// 모든 재료를 올바른 위치에 놓으면 함박정식이 완성되고 아귀 이벤트가 트리거됩니다.
///
/// [Stage 2와의 차이점]
/// Stage 2는 색상(Color)으로 매칭했지만, Stage 4는 재료 이름(itemId 문자열)으로 매칭합니다.
/// PuzzleDraggableItem.itemId와 PuzzleDropZone.requiredItemId가 같으면 정답입니다.
///
/// [버그 수정]
/// - IsSolutionCorrect()에서 slot.IsCorrect 체크가 주석 처리되어 있어
///   재료 배치 정답 여부와 무관하게 항상 퍼즐이 클리어되던 문제 수정.
///   (기획서: "순서와 강도를 맞춰 전부 진행하면 요리 완성" — 정답 조건 자체는 변경 없음)
///
/// [씬 설정]
/// 1. 조리대 위에 재료 오브젝트들 배치 (PuzzleDraggableItem + itemId 설정 + Collider)
///    예: 당근 → itemId = "carrot", 돼지고기 → itemId = "pork" 등
/// 2. 접시 위에 슬롯 오브젝트들 배치 (PuzzleDropZone + requiredItemId 설정)
///    각 슬롯이 요구하는 재료 ID를 Inspector에서 설정
/// 3. 이 스크립트를 빈 오브젝트에 붙이고 위 오브젝트들 연결
/// 4. PuzzleTrigger에서 이 스크립트를 Puzzle 슬롯에 연결
/// </summary>
public class Stage4_ToyFoodPuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	[Header("재료 오브젝트 (씬에 배치된 것들)")]
	[Tooltip("당근, 돼지고기, 계란 등 드래그 가능한 재료 오브젝트들.\n" +
			 "각각 PuzzleDraggableItem 컴포넌트와 itemId가 설정되어 있어야 합니다.")]
	[SerializeField] private List<PuzzleDraggableItem> ingredients = new List<PuzzleDraggableItem>();

	[Header("접시 슬롯 (PuzzleDropZone, 재료 놓을 위치들)")]
	[Tooltip("접시 위의 각 재료 자리. requiredItemId에 해당 재료 ID를 입력하세요.\n" +
			 "예: 당근 자리 → requiredItemId = 'carrot'")]
	[SerializeField] private List<PuzzleDropZone> plateSlots = new List<PuzzleDropZone>();

	[Header("완성 요리 오브젝트 (선택)")]
	[Tooltip("퍼즐 완료 시 나타날 완성된 함박정식 오브젝트. 없어도 됩니다.")]
	[SerializeField] private GameObject completedDishObject;

	[Tooltip("조리대 표면의 Y 좌표. 드래그 평면 기준으로 씬에서 조리대의 Y값을 넣으세요.")]
	[SerializeField] private float tableSurfaceY = 0.8f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "맛있어 보인다... 가져다줘야겠다.";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "나중에 다시 만들자.";

	// ────────────────────────────────────────────
	// 초기화
	// ────────────────────────────────────────────

	protected override void Awake()
	{
		base.Awake();
		// 슬롯에 이 컨트롤러 등록 (IDropZonePuzzle 인터페이스로)
		foreach (var slot in plateSlots)
			if (slot != null) slot.Initialize(this);

		// 완성 요리는 처음엔 숨김
		if (completedDishObject != null)
			completedDishObject.SetActive(false);
	}

	// ────────────────────────────────────────────
	// IDropZonePuzzle 구현
	// ────────────────────────────────────────────

	public void OnItemPlacedOnZone(PuzzleDropZone zone)
	{
		Debug.Log($"[FoodPuzzle] 재료 배치: {zone.requiredItemId}");
		CheckSolution();
	}

	// ────────────────────────────────────────────
	// 퍼즐 시작 / 종료
	// ────────────────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();
		Camera cam = Camera.main;
		foreach (var ingredient in ingredients)
			if (ingredient != null) ingredient.EnableDragging(cam, tableSurfaceY);

		Debug.Log("[FoodPuzzle] 레시피대로 재료를 접시에 놓아주세요.");
	}

	public override void ExitPuzzle()
	{
		ResetPuzzle();
		GameServices.UI?.ShowDialogue(speaker, exitDialogue);
		base.ExitPuzzle();
	}

	// ────────────────────────────────────────────
	// 정답 판정
	// ────────────────────────────────────────────

	/// <summary>
	/// 모든 슬롯이 각자의 requiredItemId와 일치하는 재료로 채워졌는지 확인합니다.
	/// [버그 수정] slot.IsCorrect 체크 주석 해제 — 실제 정답 검증이 동작하도록 복원.
	/// </summary>
	protected override bool IsSolutionCorrect()
	{
		if (plateSlots.Count == 0) return false;

		foreach (var slot in plateSlots)
		{
			if (slot == null) continue;
			if (!slot.IsCorrect) return false;
		}
		return true;
	}

	protected override void SolvePuzzle()
	{
		// 드래그 비활성화 (완성 후엔 못 건드리게)
		foreach (var ingredient in ingredients)
			if (ingredient != null) ingredient.DisableDragging();

		// 완성 요리 오브젝트 등장
		if (completedDishObject != null)
			completedDishObject.SetActive(true);

		GameServices.UI?.ShowDialogue(speaker, solveDialogue);
		base.SolvePuzzle(); // isSolved = true, 카메라 원위치, OnPuzzleSolved 이벤트
	}

	// ────────────────────────────────────────────
	// 리셋
	// ────────────────────────────────────────────

	private void ResetPuzzle()
	{
		foreach (var slot in plateSlots) if (slot != null) slot.RemoveItem();
		foreach (var ingredient in ingredients) if (ingredient != null) ingredient.ResetToOriginalPosition();
		Debug.Log("[FoodPuzzle] 퍼즐 리셋됨");
	}
}