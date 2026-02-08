using UnityEngine;

/// <summary>
/// 게임 상태 관리
/// </summary>
public class GameStateManager : IGameStateManager
{
	private GameState _currentState;

	public GameState CurrentState => _currentState;
	public event System.Action<GameState> OnStateChanged;

	public void ChangeState(GameState newState)
	{
		if (_currentState == newState) return;

		_currentState = newState;
		OnStateChanged?.Invoke(newState);

		Debug.Log($"[GameState] {newState}");
	}
}