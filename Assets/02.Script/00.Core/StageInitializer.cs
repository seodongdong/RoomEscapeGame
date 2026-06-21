using UnityEngine;

/// <summary>
/// 씬 시작 시 GameState를 Playing으로 전환합니다.
/// IntroSequence가 없는 스테이지(Stage2~5)에 배치하세요.
/// GameManager 프리팹 오브젝트에 붙여도 됩니다.
/// </summary>
public class StageInitializer : MonoBehaviour
{
	private void Start()
	{
		if (GameManager.Instance == null) return;

		var currentState = GameManager.Instance.CurrentState;

		// MainMenu 또는 초기 상태일 때만 Playing으로 전환
		if (currentState == GameState.MainMenu)
		{
			GameManager.Instance.ChangeState(GameState.Playing);
			Debug.Log("[StageInitializer] GameState → Playing");
		}
	}
}