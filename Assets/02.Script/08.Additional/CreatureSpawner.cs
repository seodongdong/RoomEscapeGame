using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
	[System.Serializable]
	public class CreatureSpawn
	{
		public CreatureBase creaturePrefab;
		public Transform spawnPoint;
		public float spawnDelay;
		public bool spawnOnStart = true;
	}

	[SerializeField] private int stageNumber;
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
					SpawnCreature(spawn);
				}
			}
		}
	}

	private void SpawnCreature(CreatureSpawn spawn)
	{
		if (spawn.creaturePrefab != null && spawn.spawnPoint != null)
		{
			Invoke(nameof(DelayedSpawn), spawn.spawnDelay);
		}
	}

	private void DelayedSpawn()
	{
		// 실제 스폰 로직
	}
}