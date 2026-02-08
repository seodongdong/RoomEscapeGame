using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 4스테이지: 장난감 요리 만들기 (주방)
/// 기획서: "곳곳의 레시피 보면서 조리법에 맞춰가면서 요리"
/// 완성품: 함박정식
/// </summary>
public class Stage4_ToyFoodPuzzle : PuzzleBase
{
	[System.Serializable]
	public class RecipeStep
	{
		public string ingredientId;     // 예: "carrot", "pork", "egg"
		public string actionId;         // 예: "cut", "cook", "mix"
		public int order;               // 순서
	}

	[System.Serializable]
	public class Ingredient
	{
		public string id;
		public GameObject prefab;
		public bool isCollected;
	}

	[Header("Recipe")]
	[SerializeField] private List<RecipeStep> recipeSteps;

	[Header("Ingredients")]
	[SerializeField] private List<Ingredient> availableIngredients;

	[Header("Locations")]
	[SerializeField] private Transform refrigerator;   // 냉장고
	[SerializeField] private Transform shelf;          // 선반
	[SerializeField] private Transform cookingTable;   // 조리대

	private List<string> _currentRecipe = new List<string>();
	private int _currentStep = 0;

	/// <summary>
	/// 재료 수집
	/// </summary>
	public void CollectIngredient(string ingredientId)
	{
		var ingredient = availableIngredients.Find(i => i.id == ingredientId);
		if (ingredient != null && !ingredient.isCollected)
		{
			ingredient.isCollected = true;
			Debug.Log($"[ToyFood] {ingredientId} 수집");
		}
	}

	/// <summary>
	/// 요리 단계 진행
	/// </summary>
	public void PerformCookingAction(string ingredientId, string actionId)
	{
		if (_currentStep >= recipeSteps.Count) return;

		var expectedStep = recipeSteps[_currentStep];

		if (expectedStep.ingredientId == ingredientId && expectedStep.actionId == actionId)
		{
			_currentRecipe.Add($"{ingredientId}_{actionId}");
			_currentStep++;

			Debug.Log($"[ToyFood] 단계 {_currentStep}/{recipeSteps.Count} 완료");

			CheckSolution();
		}
		else
		{
			Debug.Log("[ToyFood] 잘못된 순서!");
		}
	}

	protected override bool IsSolutionCorrect()
	{
		return _currentStep >= recipeSteps.Count;
	}

	protected override void SolvePuzzle()
	{
		base.SolvePuzzle();

		// 기획서: "완성 시점에서는 괜찮아보였는데 아귀 앞에 놓여졌을 때, 파리가 엄청 꼬인다"
		Debug.Log("[ToyFood] 함박정식 완성!");

		// 아귀에게 음식 제공 이벤트 트리거
		TriggerGhoulEvent();
	}

	private void TriggerGhoulEvent()
	{
		// 기획서: "아귀가 비명을 지르고, 몸을 비틀면서 움직임"
		var ghoul = FindAnyObjectByType<Stage4_GhoulCreature>();
		ghoul?.TriggerScream();

		// "화면 흔들림 연출 후에 화면이 페이드아웃되며 복도로 자동으로 탈출"
		StartCoroutine(EscapeSequence());
	}

	private System.Collections.IEnumerator EscapeSequence()
	{
		// 화면 흔들림
		yield return new WaitForSeconds(2f);

		// 페이드 아웃
		// TODO: 페이드 효과

		// 목각인형 획득
		var player = FindAnyObjectByType<Player>();
		var woodenDoll = new ClueItem("wooden_doll_4", "목각인형", "주방에서 획득한 목각인형");
		player.Inventory.AddItem(woodenDoll);

		// 복도로 이동
		GameManager.Instance.StageManager.CompleteStage();
	}
}