using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 인벤토리 구현
/// Dictionary로 아이템 관리, 중복 아이템 개수 추적
/// </summary>
public class PlayerInventory : IInventory
{
	private Dictionary<string, IItem> _items = new Dictionary<string, IItem>();
	private Dictionary<string, int> _itemCounts = new Dictionary<string, int>();

	#region IInventory Implementation

	public bool AddItem(IItem item)
	{
		if (!_items.ContainsKey(item.ItemId))
		{
			_items[item.ItemId] = item;
			_itemCounts[item.ItemId] = 0;
		}

		_itemCounts[item.ItemId]++;

		Debug.Log($"[Inventory] {item.ItemName} 획득! (x{_itemCounts[item.ItemId]})");
		return true;
	}

	public bool RemoveItem(IItem item)
	{
		if (!HasItem(item.ItemId)) return false;

		_itemCounts[item.ItemId]--;

		if (_itemCounts[item.ItemId] <= 0)
		{
			_items.Remove(item.ItemId);
			_itemCounts.Remove(item.ItemId);
		}

		return true;
	}

	public bool HasItem(string itemId)
	{
		return _items.ContainsKey(itemId) && _itemCounts[itemId] > 0;
	}

	public IItem GetItem(string itemId)
	{
		return _items.ContainsKey(itemId) ? _items[itemId] : null;
	}

	public int GetItemCount(string itemId)
	{
		return _itemCounts.ContainsKey(itemId) ? _itemCounts[itemId] : 0;
	}

	#endregion

	#region Additional Methods

	/// <summary>
	/// 모든 아이템 가져오기
	/// 인벤토리 UI에서 사용
	/// </summary>
	public List<IItem> GetAllItems()
	{
		List<IItem> allItems = new List<IItem>();

		foreach (var kvp in _items)
		{
			allItems.Add(kvp.Value);
		}

		return allItems;
	}

	/// <summary>
	/// 모든 아이템과 개수를 Dictionary로 가져오기
	/// </summary>
	public Dictionary<string, int> GetAllItemCounts()
	{
		return new Dictionary<string, int>(_itemCounts);
	}

	/// <summary>
	/// 전체 아이템 개수
	/// </summary>
	public int GetTotalItemCount()
	{
		int total = 0;
		foreach (var count in _itemCounts.Values)
		{
			total += count;
		}
		return total;
	}

	/// <summary>
	/// 인벤토리 비우기
	/// </summary>
	public void Clear()
	{
		_items.Clear();
		_itemCounts.Clear();
		Debug.Log("[Inventory] 인벤토리 초기화");
	}

	#endregion
}