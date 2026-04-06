using UnityEngine;
/// <summary>
/// 게임 상태 관리 인터페이스
/// Playing, Puzzle, Chase, Dialogue 등 상태 전환
/// </summary>
public interface IGameStateManager
{
	GameState CurrentState { get; }

	void ChangeState(GameState newState);

	event System.Action<GameState> OnStateChanged;
}