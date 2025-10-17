using UnityEngine;
using System.Collections;

// 스테이지 매니저 인터페이스 구현 클래스

public class StageManager : IStageManager
{
    private int _currentStage = 1;
    
    public int CurrentStage => _currentStage;
    public event System.Action<int> OnStateChanged;     // 스테이지 상태가 변경되었을 때 발생하는 이벤트

    public void LoadStage(int stageNumber)
    {
        _currentStage = stageNumber;                    // 현재 스테이지 업데이트
        OnStateChanged?.Invoke(_currentStage);          // 이벤트 발생
        Debug.Log($"스테이지 로드 : {_currentStage}");
    }

    public void CompleteStage()
    {
        _currentStage++;                                // 스테이지 완료 후 다음 스테이지로 이동
        OnStateChanged?.Invoke(_currentStage);          // 이벤트 발생
        Debug.Log($"스테이지 완료! 다음 스테이지로 이동 : {_currentStage}");
    }
}
