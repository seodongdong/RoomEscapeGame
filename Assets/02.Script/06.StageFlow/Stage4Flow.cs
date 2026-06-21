using UnityEngine;

/// <summary>
/// 4스테이지(주방) 진행 흐름 추적.
///
/// 설계 흐름:
/// 입장 → 레시피메모수집 → 요리퍼즐 → 요리완성 → 아귀이벤트
///   → 복도탈출 → 완료
///
/// [연결 방법 - Inspector]
/// foodPuzzle, ghoulCreature 슬롯에 씬의 해당 오브젝트를 연결하세요.
/// </summary>
public class Stage4Flow : StageFlowBase
{
	[Header("연결 — 기존 오브젝트 그대로 참조")]
	[SerializeField] private Stage4_ToyFoodPuzzle foodPuzzle;
	[SerializeField] private Stage4_GhoulCreature ghoulCreature;

	protected override void Awake()
	{
		base.Awake();

		if (foodPuzzle != null)
			foodPuzzle.OnPuzzleSolved += () => SetStep("DishCompleted");
	}

	private void Update()
	{
		if (currentStep == "Entering")
			SetStep("RecipeNotesCollecting");

		if (currentStep == "RecipeNotesCollecting" && foodPuzzle != null && !foodPuzzle.IsSolved)
			SetStep("CookingPuzzleEntered");

		if (currentStep == "DishCompleted")
			SetStep("GhoulEvent");
	}
}
