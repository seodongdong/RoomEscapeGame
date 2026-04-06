using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인 메뉴 관리
/// </summary>
public class MainMenuManager : MonoBehaviour
{
	[Header("UI Buttons")]
	[SerializeField] private Button startButton;
	[SerializeField] private Button loadButton;
	[SerializeField] private Button optionsButton;
	[SerializeField] private Button quitButton;

	[Header("Panels")]
	[SerializeField] private GameObject mainPanel;
	[SerializeField] private GameObject optionsPanel;

	[Header("Settings")]
	[SerializeField] private string firstSceneName = "Stage1_LivingRoom";

	private SaveSystem _saveSystem;

	private void Start()
	{
		_saveSystem = FindAnyObjectByType<SaveSystem>();

		InitializeButtons();
		CheckLoadButtonState();
	}

	private void InitializeButtons()
	{
		if (startButton != null)
		{
			startButton.onClick.AddListener(StartNewGame);
		}

		if (loadButton != null)
		{
			loadButton.onClick.AddListener(LoadGame);
		}

		if (optionsButton != null)
		{
			optionsButton.onClick.AddListener(OpenOptions);
		}

		if (quitButton != null)
		{
			quitButton.onClick.AddListener(QuitGame);
		}
	}

	private void CheckLoadButtonState()
	{
		if (loadButton != null && _saveSystem != null)
		{
			loadButton.interactable = _saveSystem.HasSaveFile();
		}
	}

	public void StartNewGame()
	{
		GameManager.Instance.StateManager.ChangeState(GameState.Playing);

		// firstSceneName 직접 사용
		if (!string.IsNullOrEmpty(firstSceneName))
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(firstSceneName);
		}
		else
		{
			GameManager.Instance.StageManager.LoadStage(1);
		}
	}

	public void LoadGame()
	{
		if (_saveSystem != null && _saveSystem.HasSaveFile())
		{
			GameData data = _saveSystem.LoadGame();

			// 저장된 데이터로 게임 복원
			GameManager.Instance.StageManager.LoadStage(data.currentStage);

			// 플레이어 위치, 체력 등은 해당 씬에서 복원
		}
		else
		{
			Debug.Log("[MainMenu] 저장 파일이 없습니다.");
		}
	}

	public void OpenOptions()
	{
		if (mainPanel != null)
		{
			mainPanel.SetActive(false);
		}

		if (optionsPanel != null)
		{
			optionsPanel.SetActive(true);
		}
	}

	public void CloseOptions()
	{
		if (optionsPanel != null)
		{
			optionsPanel.SetActive(false);
		}

		if (mainPanel != null)
		{
			mainPanel.SetActive(true);
		}
	}

	public void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
}