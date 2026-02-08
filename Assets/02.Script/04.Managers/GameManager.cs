using UnityEngine;

/// <summary>
/// 게임 매니저 (싱글톤)
/// </summary>
public class GameManager : MonoBehaviour
{
	private static GameManager _instance;
	public static GameManager Instance => _instance;

	private IGameStateManager _stateManager;
	private IStageManager _stageManager;
	private IClueTracker _clueTracker;
	private IEndingManager _endingManager;

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

	private void InitializeManagers()
	{
		_stateManager = new GameStateManager();
		_stageManager = new StageManager();
		_clueTracker = new ClueTracker();
		_endingManager = new EndingManager();

		Debug.Log("[GameManager] 초기화 완료");
	}

	public IGameStateManager StateManager => _stateManager;
	public IStageManager StageManager => _stageManager;
	public IClueTracker ClueTracker => _clueTracker;
	public IEndingManager EndingManager => _endingManager;
}