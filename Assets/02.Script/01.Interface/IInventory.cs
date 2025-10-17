using UnityEngine;

// 플레이어의 인벤토리 시스템 인터페이스

public interface IInventory
{
    bool AddItem(IItem item);               // 아이템 추가 
    bool RemoveItem(IItem item);            // 아이템 제거
    bool HasItem(string itemId);            // 아이템 보유 여부 확인
    IItem GetItem(string itemId);           // 아이템 정보 가져오기
    int GetItemCount(string itemId);        // 아이템 개수 가져오기
}
