using UnityEngine;

/// <summary>
/// 엔딩 관리 인터페이스
/// 기획서: 게임오버 / 노말 / 진엔딩
/// </summary>
public interface IEndingManager
{
	EndingType CheckEndingConditions(IInventory inventory, bool girlRescued, bool hasCamcorder);
	void TriggerEnding(EndingType endingType);
}