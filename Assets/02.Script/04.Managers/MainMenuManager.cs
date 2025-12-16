using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	public void StartNewGame()
	{
		GameManager.Instance.StateManager.ChangeState(GameState.Playing);
		GameManager.Instance.StageManager.LoadStage(1);
	}

	public void LoadGame()
	{
		var saveSystem = FindAnyObjectByType<SaveSystem>();
		if (saveSystem != null && saveSystem.HasSaveFile())
		{
			GameData data = saveSystem.LoadGame();

			// 저장된 데이터로 게임 복원
			GameManager.Instance.StageManager.LoadStage(data.currentStage);

			// 플레이어 위치, 체력 등 복원
			var player = FindAnyObjectByType<Player>();
			if (player != null)
			{
				player.transform.position = data.playerPosition;
				player.Health.Heal(data.health - player.Health.CurrentHealth);
			}
		}
		else
		{
			Debug.Log("저장 파일이 없습니다.");
		}
	}

	public void QuitGame()
	{
		Application.Quit();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}
}
