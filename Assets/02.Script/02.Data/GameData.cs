using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
	public int currentStage;
	public string sceneName;
	public string savedDisplayName;
	public Vector3 playerPosition;
	public List<string> collectedClues;
	public int health;
	public bool[] solvedPuzzles;
	public bool hasCamcorder;
	public float playTimeSeconds;
	public bool hasFlashlight;

	// 오브젝트 상태 저장 (문/퍼즐 등)
	public List<string> savedObjectIds;
	public List<string> savedObjectStates;

	// ★ 추가: 인벤토리 아이템 표시 정보 저장
	// collectedClues에 itemId만 있으면 복원 시 이름/설명을 모름
	// 여기에 id→표시정보를 함께 저장해서 완전히 복원
	public List<string> inventoryItemIds;
	public List<string> inventoryItemTitles;
	public List<string> inventoryItemDescriptions;
	public List<string> inventoryItemTypes; // "Document" or "UsableItem"
	public List<string> inventoryItemDates;

	public GameData()
	{
		collectedClues = new List<string>();
		solvedPuzzles = new bool[5];
		hasCamcorder = false;
		playTimeSeconds = 0f;
		sceneName = "";
		savedDisplayName = "";
		hasFlashlight = false;
		savedObjectIds = new List<string>();
		savedObjectStates = new List<string>();
		inventoryItemIds = new List<string>();
		inventoryItemTitles = new List<string>();
		inventoryItemDescriptions = new List<string>();
		inventoryItemTypes = new List<string>();
		inventoryItemDates = new List<string>();
	}

	public void SetObjectState(string id, string stateJson)
	{
		int idx = savedObjectIds.IndexOf(id);
		if (idx >= 0)
			savedObjectStates[idx] = stateJson;
		else
		{
			savedObjectIds.Add(id);
			savedObjectStates.Add(stateJson);
		}
	}

	public string GetObjectState(string id)
	{
		int idx = savedObjectIds.IndexOf(id);
		return idx >= 0 ? savedObjectStates[idx] : null;
	}

	/// <summary>인벤토리 아이템 표시 정보를 저장합니다.</summary>
	public void AddInventoryItem(string itemId, string title, string description, string itemType, string date)
	{
		int idx = inventoryItemIds.IndexOf(itemId);
		if (idx >= 0)
		{
			inventoryItemTitles[idx] = title;
			inventoryItemDescriptions[idx] = description;
			inventoryItemTypes[idx] = itemType;
			inventoryItemDates[idx] = date;
		}
		else
		{
			inventoryItemIds.Add(itemId);
			inventoryItemTitles.Add(title);
			inventoryItemDescriptions.Add(description);
			inventoryItemTypes.Add(itemType);
			inventoryItemDates.Add(date);
		}
	}

	/// <summary>저장된 인벤토리 아이템 표시 정보를 가져옵니다.</summary>
	public InventoryItemData GetInventoryItemData(string itemId)
	{
		int idx = inventoryItemIds.IndexOf(itemId);
		if (idx < 0) return null;

		return new InventoryItemData
		{
			itemId = itemId,
			title = inventoryItemTitles[idx],
			description = inventoryItemDescriptions[idx],
			itemType = inventoryItemTypes[idx] == "Document" ? ItemType.Document : ItemType.UsableItem,
			date = inventoryItemDates[idx]
		};
	}
}