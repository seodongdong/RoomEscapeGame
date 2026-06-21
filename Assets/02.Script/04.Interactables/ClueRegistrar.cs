using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 단서/아이템 획득 시 필요한 3중 등록(PlayerInventory + InventoryUI_Complete + ClueTracker)을
/// 한 곳에서 처리합니다.
///
/// [기존 문제]
/// DiaryClue, UsableItemClue, PuzzleSolveDoor(목각인형 지급부), AltarIncense,
/// ObjectViewer3D 5곳에서 거의 동일한 9~10줄짜리 블록을 각자 복붙해서 썼습니다.
/// 한 곳에서 등록 순서나 누락이 생기면 다른 4곳은 멋대로 다른 동작을 하게 됩니다.
///
/// [사용법]
/// 기존:
///   player.Inventory.AddItem(new ClueItem(id, name, desc));
///   var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
///   inventoryUI?.AddItem(new InventoryItemData { ... });
///   GameManager.Instance?.ClueTracker.RegisterClue(id);
///
/// 변경 후:
///   ClueRegistrar.RegisterDocument(player, id, name, date, description, pages);
///   또는
///   ClueRegistrar.RegisterUsableItem(player, id, name, date, description, itemPrefab);
///
/// [씬 배치]
/// 정적 클래스이므로 씬 배치가 필요 없습니다.
/// 내부에서 InventoryUI_Complete를 GameServices가 아니라 직접 찾는 이유는
/// GameServices가 Core 4종(UI/Audio/Player/Save)만 책임지기로 했기 때문입니다.
/// InventoryUI_Complete 탐색은 호출 빈도가 "단서 획득 시점"
/// 으로 한정되어 있어 성능 영향이 미미합니다.
/// </summary>
public static class ClueRegistrar
{
	/// <summary>
	/// 문서류 단서(일기, 편지, 신문 등) 등록.
	/// 기획서 분류: "기록용 단서" — 상호작용 시 사라지지 않고 인벤토리에 등록.
	/// </summary>
	public static void RegisterDocument(
		IPlayer player,
		string itemId,
		string itemName,
		string itemDate,
		string description,
		List<string> pages = null)
	{
		if (player.Inventory.HasItem(itemId))
			return; // 이미 등록된 단서는 중복 등록하지 않음

		player.Inventory.AddItem(new ClueItem(itemId, itemName, description));
		GameManager.Instance?.ClueTracker.RegisterClue(itemId);

		var inventoryUI = Object.FindAnyObjectByType<InventoryUI_Complete>(FindObjectsInactive.Include);
		inventoryUI?.AddItem(new InventoryItemData
		{
			itemId = itemId,
			title = itemName,
			date = itemDate,
			itemType = ItemType.Document,
			description = description,
			pages = pages != null ? new List<string>(pages) : null
		});

		Debug.Log($"[ClueRegistrar] 문서 등록: {itemName} ({itemId})");
	}

	/// <summary>
	/// 사용 가능한 아이템(열쇠, 도구 등) 등록.
	/// 기획서 분류: "수집 및 사용 가능 단서" — 상호작용 시 사라지며 인벤토리에 등록.
	/// </summary>
	public static void RegisterUsableItem(
		IPlayer player,
		string itemId,
		string itemName,
		string itemDate,
		string description,
		GameObject itemPrefab = null)
	{
		if (player.Inventory.HasItem(itemId))
			return;

		player.Inventory.AddItem(new ClueItem(itemId, itemName, description));
		GameManager.Instance?.ClueTracker.RegisterClue(itemId);

		var inventoryUI = Object.FindAnyObjectByType<InventoryUI_Complete>(FindObjectsInactive.Include);
		inventoryUI?.AddItem(new InventoryItemData
		{
			itemId = itemId,
			title = itemName,
			date = itemDate,
			itemType = ItemType.UsableItem,
			description = description,
			itemPrefab = itemPrefab
		});

		Debug.Log($"[ClueRegistrar] 아이템 등록: {itemName} ({itemId})");
	}

	/// <summary>
	/// 인벤토리에 등록하지 않고 ClueTracker에만 집계하고 싶을 때(예: 환경 단서,
	/// "살펴보기용 단서" 중 인벤토리 등록이 필요 없는 경우)에 사용합니다.
	/// </summary>
	public static void RegisterClueOnly(string itemId)
	{
		if (string.IsNullOrEmpty(itemId)) return;
		GameManager.Instance?.ClueTracker.RegisterClue(itemId);
	}
}
