using System.Collections.Generic;

/// <summary>
/// 인벤토리 시스템 인터페이스
/// 아이템(단서) 추가/제거/확인 기능
/// </summary>
public interface IInventory
{
	// 기본 메서드
	bool AddItem(IItem item);
	bool RemoveItem(IItem item);
	bool HasItem(string itemId);
	IItem GetItem(string itemId);
	int GetItemCount(string itemId);

	// 추가 메서드
	List<IItem> GetAllItems();
	Dictionary<string, int> GetAllItemCounts();
	int GetTotalItemCount();
	void Clear();
}