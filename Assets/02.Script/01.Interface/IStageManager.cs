using UnityEngine;

// 스테이지 관리 인터페이스

public interface IStageManager
{
    int CurrentStage { get; }
    void LoadStage(int stageNumber);
    void CompleteStage();
    event System.Action<int> OnStateChanged;  // 스테이지 상태가 변경되었을 때 발생하는 이벤트
}
