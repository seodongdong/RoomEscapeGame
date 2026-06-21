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
	[Tooltip("더 이상 사용하지 않습니다. StartNewGame()이 StageManager.LoadStage(1)을 직접 호출하도록 변경되어, 이 필드는 무시됩니다. 1스테이지 씬을 바꾸려면 StageManager.cs의 GetSceneName()을 수정하세요.")]
	[SerializeField] private string firstSceneName = "Stage1_LivingRoom";

	[Header("저장 슬롯 UI (불러오기 버튼이 엶)")]
	[SerializeField] private SaveSlotUI saveSlotUI;

	private SaveSystem _saveSystem;

	private void Start()
	{
		_saveSystem = FindAnyObjectByType<SaveSystem>();

		InitializeButtons();
		CheckLoadButtonState();
	}

	private void Update()
	{
		// 옵션 패널이 열려있을 때 ESC로 닫기
		if (optionsPanel != null && optionsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
		{
			CloseOptions();
		}
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

	/// <summary>
	/// ★ 변경: 슬롯 0번만 확인하던 것을 4개 슬롯 전체 확인으로 변경.
	/// 하나라도 저장 데이터가 있으면 "불러오기" 버튼을 활성화합니다.
	/// </summary>
	private void CheckLoadButtonState()
	{
		if (loadButton == null || _saveSystem == null) return;

		bool anySaveExists = false;
		for (int i = 0; i < SaveSystem.SLOT_COUNT; i++)
		{
			if (_saveSystem.HasSaveFile(i))
			{
				anySaveExists = true;
				break;
			}
		}

		loadButton.interactable = anySaveExists;
	}

	/// <summary>
	/// "시작" 버튼 클릭 시 호출됩니다.
	/// ★ 변경: SceneManager.LoadScene(firstSceneName)을 직접 호출하던 것을
	/// GameManager.Instance.StageManager.LoadStage(1)로 교체했습니다.
	///
	/// 기존 방식은 firstSceneName("Stage1_LivingRoom")이 실제 빌드의
	/// 씬 파일명(예: "02_Stage1_LivingRoom")과 정확히 일치하지 않으면
	/// SceneManager.LoadScene이 조용히 실패하고 아무 일도 일어나지
	/// 않았습니다(예외를 던지지 않아 알아차리기 어려움).
	///
	/// StageManager.LoadStage(1)을 쓰면 StageManager에 이미 추가된
	/// 폴백 로직(이름이 안 맞으면 Build Settings를 순회해 핵심 키워드로
	/// 다시 찾는 로직)을 그대로 활용할 수 있습니다.
	/// </summary>
	public void StartNewGame()
	{
		if (GameManager.Instance == null)
		{
			Debug.LogError("[MainMenu] GameManager.Instance가 없습니다. " +
				"00_Boot 씬을 거치지 않고 메인메뉴를 직접 실행하면 " +
				"GameManager가 생성되지 않아 게임을 시작할 수 없습니다.");
			return;
		}

		GameManager.Instance.ChangeState(GameState.Playing);
		GameManager.Instance.StageManager.LoadStage(1);
	}

	/// <summary>
	/// "불러오기" 버튼 클릭 시 호출됩니다.
	/// ★ 변경: SaveSystem.LoadGame()(슬롯 0 고정)을 직접 부르는 대신,
	/// SaveSlotUI의 저장 슬롯 선택 패널을 열어 사용자가 슬롯을 직접
	/// 고를 수 있게 합니다. 실제 불러오기 처리는 SaveSlotUI 쪽
	/// OnLoadClicked()가 그대로 담당합니다(메인메뉴 씬에서도 동일하게 동작).
	/// </summary>
	public void LoadGame()
	{
		if (saveSlotUI != null)
		{
			saveSlotUI.OpenPanel();
		}
		else
		{
			Debug.LogError("[MainMenu] saveSlotUI가 연결되어 있지 않습니다. Inspector에서 연결하세요.");
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