using UnityEngine;

/// <summary>
/// 아이템(단서) 인터페이스
/// 모든 수집 가능한 오브젝트의 기본 정보 정의
/// </summary>
public interface IItem
{
	string ItemId { get; }
	string ItemName { get; }
	string Description { get; }
	Sprite Icon { get; }
	bool IsClue { get; }
}