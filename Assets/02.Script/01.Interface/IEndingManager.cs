using UnityEngine;

// 엔딩 매니저 인터페이스

public interface IEndingManager
{
    EndingType CheckEndingConditions(IInventory inventory, bool girlRescued);
    void TriggerEnding(EndingType endingType);
}
