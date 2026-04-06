using UnityEngine;

// 게임 진행 상태 관리 인터페이스

public interface IGameStateManager
{
    GameState CurrentState { get; }
    void ChangeState(GameState newState);
    event System.Action<GameState> OnStateChanged;  // 게임 상태가 변경되었을 때 발생하는 이벤트
}
