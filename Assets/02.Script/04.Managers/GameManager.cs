using UnityEngine;

/// <summary>
/// 게임 매니저 (싱글톤)
///
/// [추가]
/// - PlayTimeSeconds: 게임 시작부터 누적된 플레이 시간(초).
///   SaveSlotUI가 저장 시점에 이 값을 GameData.playTimeSeconds에 기록합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
	private static GameManager _instance;
	public static GameManager Instance => _instance;

	private IGameStateManager _stateManager;
	private IStageManager _stageManager;
	private IClueTracker _clueTracker;
	private IEndingManager _endingManager;

	// ★ 추가: 누적 플레이 시간
	private float _playTimeSeconds = 0f;
	public float PlayTimeSeconds => _playTimeSeconds;

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);

		InitializeManagers();
	}

	private void Update()
	{
		// ★ 추가: 일시정지 상태가 아닐 때만 누적 (Paused면 멈춤)
		if (_stateManager != null && _stateManager.CurrentState != GameState.Paused)
			_playTimeSeconds += Time.deltaTime;
	}

	private void InitializeManagers()
	{
		_stateManager = new GameStateManager();
		_stageManager = new StageManager();
		_clueTracker = new ClueTracker();
		_endingManager = new EndingManager();

		Debug.Log("[GameManager] 초기화 완료");
	}

	/// <summary>
	/// 불러오기 시 저장된 시점의 플레이 시간으로 복원합니다.
	/// </summary>
	public void SetPlayTime(float seconds)
	{
		_playTimeSeconds = seconds;
	}

	public IGameStateManager StateManager => _stateManager;
	public IStageManager StageManager => _stageManager;
	public IClueTracker ClueTracker => _clueTracker;
	public IEndingManager EndingManager => _endingManager;
}