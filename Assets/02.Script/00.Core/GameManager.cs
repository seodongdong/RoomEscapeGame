using UnityEngine;

/// <summary>
/// 게임 매니저 (싱글톤) — GameStateManager 통합판
///
/// [리팩토링 변경사항]
/// GameStateManager는 MonoBehaviour가 아닌 순수 클래스였고, GameManager
/// 단 하나만 이를 생성해 1:1로 보관하고 있었습니다. 독립적으로 재사용되거나
/// 테스트되는 곳이 없어 이번 작업에서 GameManager에 통합했습니다.
///
/// StageManager / ClueTracker / EndingManager는 통합하지 않고 그대로
/// 별도 클래스로 유지합니다. 책임이 명확히 다르고, 합치면 GameManager가
/// 너무 비대해지기 때문입니다.
///
/// [기존 호출부 변경]
/// 기존: GameManager.Instance.StateManager.ChangeState(GameState.Playing)
/// 변경: GameManager.Instance.ChangeState(GameState.Playing)
/// 코드베이스 전체의 .StateManager. 호출부를 이번 작업에서 전부 교체했습니다.
///
/// [추가 - 씬 전환 데이터 임시 보관]
/// SetPendingLoadData / ConsumePendingLoadData:
/// 씬 전환 전에 불러올 GameData를 잠깐 보관합니다.
/// SaveLoader가 새 씬 시작 시 이 데이터를 꺼내 위치/인벤토리/단서 상태를 복원합니다.
/// </summary>
public class GameManager : MonoBehaviour, IGameStateManager
{
	private static GameManager _instance;
	public static GameManager Instance => _instance;

	// ── GameStateManager 흡수 ────────────────────────────────
	private GameState _currentState;
	public GameState CurrentState => _currentState;
	public event System.Action<GameState> OnStateChanged;

	public void ChangeState(GameState newState)
	{
		if (_currentState == newState) return;

		_currentState = newState;
		OnStateChanged?.Invoke(newState);

		Debug.Log($"[GameManager] 상태 변경: {newState}");
	}

	// ── 기존 서브 매니저 (통합하지 않고 유지) ─────────────────
	private IStageManager _stageManager;
	private IClueTracker _clueTracker;
	private IEndingManager _endingManager;

	private float _playTimeSeconds = 0f;
	public float PlayTimeSeconds => _playTimeSeconds;

	// 씬 전환 간 임시 보관용 (DontDestroyOnLoad 객체이므로 씬이 바뀌어도 유지됨)
	private GameData _pendingLoadData;

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
		if (_currentState != GameState.Paused)
			_playTimeSeconds += Time.deltaTime;
	}

	private void InitializeManagers()
	{
		_stageManager = new StageManager();
		_clueTracker = new ClueTracker();
		// EndingManager가 ClueTracker를 직접 참조하도록 생성자로 주입합니다.
		// (기존에는 ClueTracker가 "15개 단서 수집"을 추적하면서도 실제 엔딩
		//  판정은 이를 참조하지 않아 두 시스템이 단절되어 있었습니다.
		//  엔딩 판정 조건 자체를 바꾸는 것은 기획 영역이므로 여기서는
		//  "연결 통로"만 만들고, 실제로 그 값을 쓸지는 기획 결정에 맡깁니다.)
		_endingManager = new EndingManager(_clueTracker);

		Debug.Log("[GameManager] 초기화 완료");
	}

	public void SetPlayTime(float seconds)
	{
		_playTimeSeconds = seconds;
	}

	/// <summary>불러올 데이터를 등록합니다. 씬 로드 직전에 호출하세요.</summary>
	public void SetPendingLoadData(GameData data)
	{
		_pendingLoadData = data;
	}

	/// <summary>
	/// 새 씬에서 SaveLoader가 호출합니다.
	/// 한 번 꺼내면 내부 값을 비워, 다음 일반 진입 때 잘못 재사용되지 않도록 합니다.
	/// </summary>
	public GameData ConsumePendingLoadData()
	{
		var data = _pendingLoadData;
		_pendingLoadData = null;
		return data;
	}

	public bool HasPendingLoadData => _pendingLoadData != null;

	public IStageManager StageManager => _stageManager;
	public IClueTracker ClueTracker => _clueTracker;
	public IEndingManager EndingManager => _endingManager;
}
