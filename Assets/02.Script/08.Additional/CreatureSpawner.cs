using UnityEngine;
using System.Collections;

/// <summary>
/// 크리처 스폰 관리
/// 스테이지별로 크리처를 자동 스폰
/// </summary>
public class CreatureSpawner : MonoBehaviour
{
	[System.Serializable]
	public class CreatureSpawn
	{
		public CreatureBase creaturePrefab;
		public Transform spawnPoint;
		public float spawnDelay;
		public bool spawnOnStart = true;
		public bool spawnOnPuzzleSolved;
		public string triggerPuzzleId;
	}

	[Header("Stage Settings")]
	[SerializeField] private int stageNumber;

	[Header("Creatures")]
	[SerializeField] private CreatureSpawn[] creatures;

	private void Start()
	{
		// 현재 스테이지에 맞는 크리처만 스폰
		if (GameManager.Instance.StageManager.CurrentStage == stageNumber)
		{
			foreach (var spawn in creatures)
			{
				if (spawn.spawnOnStart)
				{
					StartCoroutine(SpawnCreatureDelayed(spawn));
				}
			}
		}
	}

	private IEnumerator SpawnCreatureDelayed(CreatureSpawn spawn)
	{
		yield return new WaitForSeconds(spawn.spawnDelay);

		SpawnCreature(spawn);
	}

	private void SpawnCreature(CreatureSpawn spawn)
	{
		if (spawn.creaturePrefab == null || spawn.spawnPoint == null)
		{
			Debug.LogWarning("[CreatureSpawner] Prefab 또는 SpawnPoint가 없습니다!");
			return;
		}

		CreatureBase creature = Instantiate(
			spawn.creaturePrefab,
			spawn.spawnPoint.position,
			spawn.spawnPoint.rotation
		);

		Debug.Log($"[CreatureSpawner] {creature.name} 스폰 완료");
	}

	/// <summary>
	/// 퍼즐 해결 시 크리처 스폰
	/// </summary>
	public void OnPuzzleSolved(string puzzleId)
	{
		foreach (var spawn in creatures)
		{
			if (spawn.spawnOnPuzzleSolved && spawn.triggerPuzzleId == puzzleId)
			{
				SpawnCreature(spawn);
			}
		}
	}

	/// <summary>
	/// 특정 크리처 수동 스폰
	/// </summary>
	public void SpawnCreatureByIndex(int index)
	{
		if (index >= 0 && index < creatures.Length)
		{
			SpawnCreature(creatures[index]);
		}
	}

	/// <summary>
	/// 모든 크리처 스폰
	/// </summary>
	public void SpawnAllCreatures()
	{
		foreach (var spawn in creatures)
		{
			SpawnCreature(spawn);
		}
	}
}