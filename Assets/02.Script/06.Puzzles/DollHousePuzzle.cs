using UnityEngine;
using System.Collections.Generic;

public class DollHousePuzzle : PuzzleBase
{
	[System.Serializable]
	public class DollItem
	{
		public string itemId;
		public Transform targetSlot;
		public GameObject prefab;
	}

	[SerializeField] private List<DollItem> requiredItems;
	private Dictionary<string, bool> _placedItems = new Dictionary<string, bool>();

	private void Awake()
	{
		foreach (var item in requiredItems)
		{
			_placedItems[item.itemId] = false;
		}
	}

	public void PlaceItem(string itemId)
	{
		if (_placedItems.ContainsKey(itemId))
		{
			_placedItems[itemId] = true;
			Debug.Log($"아이템 배치: {itemId}");
			CheckSolution();
		}
	}

	protected override bool IsSolutionCorrect()
	{
		foreach (var placed in _placedItems.Values)
		{
			if (!placed) return false;
		}
		return true;
	}
}