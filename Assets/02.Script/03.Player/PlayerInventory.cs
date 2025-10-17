using UnityEngine;
using System.Collections.Generic;

// 플레이어 인벤토리를 관리하는 클래스

public class PlayerInventory : IInventory
{
    // 아이템 저장을 위한 딕셔너리
    private Dictionary<string, IItem> _items = new Dictionary<string, IItem>();
    private Dictionary<string, int> _itemCounts = new Dictionary<string, int>();

    // 아이템 추가
    public bool AddItem(IItem item)
    {
        if (!_items.ContainsKey(item.ItemId))   // 아이템이 없으면 새로 추가
        {
            _items[item.ItemId] = item;     // 아이템 등록
            _itemCounts[item.ItemId] = 0;   // 개수 초기화
        }
        
        _itemCounts[item.ItemId]++;         // 아이템 개수 증가
        Debug.Log($"{item.ItemName} 획득! (x{_itemCounts[item.ItemId]})");
        return true;
    }

    // 아이템 제거
    public bool RemoveItem(IItem item)
    {
        if (!HasItem(item.ItemId)) return false;    // 아이템이 없으면 제거 불가

        _itemCounts[item.ItemId]--;                 // 아이템 개수 감소
        if (_itemCounts[item.ItemId] <= 0)          // 개수가 0이 되면 아이템 제거
        {
            _items.Remove(item.ItemId);             // 아이템 등록 해제
            _itemCounts.Remove(item.ItemId);        // 개수 정보 제거
        }

        return true;
    }

    // 아이템 보유 여부 확인
    public bool HasItem(string itemId)
    {
        return _items.ContainsKey(itemId) && _itemCounts[itemId] > 0;
    }

    // 아이템 정보 가져오기
    public IItem GetItem(string itemId)
    {
        return _items.ContainsKey(itemId) ? _items[itemId] : null;
    }

    // 아이템 개수 가져오기
    public int GetItemCount(string itemId)
    {
        return _itemCounts.ContainsKey(itemId) ? _itemCounts[itemId] : 0;
    }
}
