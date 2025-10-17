using UnityEngine;

// 아이템 인터페이스

public interface IItem
{
    string ItemId { get; }
    string ItemName { get; }
    string Description { get; }
    Sprite Icon { get; }
    bool IsClue { get; }    // 단서 아이템 여부
}
