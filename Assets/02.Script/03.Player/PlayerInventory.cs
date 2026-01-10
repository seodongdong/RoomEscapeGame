using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : IInventory
{
    private Dictionary<string, IItem> _items = new Dictionary<string, IItem>();
    private Dictionary<string, int> _itemCounts = new Dictionary<string, int>();

    public bool AddItem(IItem item)
    {
        if (!_items.ContainsKey(item.ItemId))
        {
            _items[item.ItemId] = item;
            _itemCounts[item.ItemId] = 0;
        }
        
        _itemCounts[item.ItemId]++;
        Debug.Log($"{item.ItemName} 획득! (x{_itemCounts[item.ItemId]})");
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

    // ⭐ 모든 아이템 반환 메서드
    public List<IItem> GetAllItems()
    {
        List<IItem> itemList = new List<IItem>();
        
        Debug.Log($"=== GetAllItems 호출 ===");
        Debug.Log($"Dictionary 크기: {_items.Count}");
        
        foreach (var kvp in _items)
        {
            Debug.Log($"  키: {kvp.Key}, 값: {kvp.Value.ItemName}, IsClue: {kvp.Value.IsClue}");
            
            // 모든 아이템 추가 (IsClue 체크는 나중에)
            itemList.Add(kvp.Value);
        }
        
        Debug.Log($"반환 아이템 개수: {itemList.Count}");
        return itemList;
    }
}