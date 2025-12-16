using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FoodMiniGame : PuzzleBase
{
	[System.Serializable]
	public class FoodItem
	{
		public GameObject prefab;
		public int calories;
		public bool isGood;
	}

	[SerializeField] private List<FoodItem> foodItems;
	[SerializeField] private Transform spawnArea;
	[SerializeField] private float spawnInterval = 1f;
	[SerializeField] private int targetCalories = 500;
	[SerializeField] private float timeLimit = 60f;

	private int _currentCalories;
	private float _elapsedTime;
	private bool _isPlaying;

	public override void StartPuzzle()
	{
		base.StartPuzzle();
		_currentCalories = 0;
		_elapsedTime = 0;
		_isPlaying = true;
		StartCoroutine(SpawnFoodRoutine());
	}

	private IEnumerator SpawnFoodRoutine()
	{
		while (_isPlaying)
		{
			SpawnRandomFood();
			yield return new WaitForSeconds(spawnInterval);
		}
	}

	private void SpawnRandomFood()
	{
		int randomIndex = Random.Range(0, foodItems.Count);
		var foodItem = foodItems[randomIndex];

		Vector3 spawnPos = new Vector3(
			Random.Range(spawnArea.position.x - 5, spawnArea.position.x + 5),
			spawnArea.position.y,
			spawnArea.position.z
		);

		GameObject food = Instantiate(foodItem.prefab, spawnPos, Quaternion.identity);
		food.GetComponent<FoodDrop>()?.Initialize(foodItem.calories, foodItem.isGood);
	}

	public void CollectFood(int calories, bool isGood)
	{
		if (isGood)
		{
			_currentCalories += calories;
		}
		else
		{
			_currentCalories -= calories;
		}

		CheckSolution();
	}

	private void Update()
	{
		if (!_isPlaying) return;

		_elapsedTime += Time.deltaTime;

		if (_elapsedTime >= timeLimit)
		{
			_isPlaying = false;
			CheckSolution();
		}
	}

	protected override bool IsSolutionCorrect()
	{
		return _currentCalories >= targetCalories;
	}
}