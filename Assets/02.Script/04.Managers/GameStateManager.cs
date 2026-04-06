using UnityEngine;
using System.Collections;

// 게임 진행 상태 관리 매니저 클래스

public class GameStateManager : IGameStateManager
{
    private GameState _currentState;

    public GameState CurrentState => _currentState; 
    public event System.Action<GameState> OnStateChanged;       // 게임 상태가 변경되었을 때 발생하는 이벤트

    public void ChangeState(GameState newState)
    {
        if (_currentState == newState) return;      // 상태가 동일하면 변경하지 않음

        _currentState = newState;                   // 현재 상태 업데이트
        OnStateChanged?.Invoke(newState);           // 이벤트 발생
        Debug.Log($"게임 상태 변경 : {newState}");      
    }
}
