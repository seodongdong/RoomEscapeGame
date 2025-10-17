using UnityEngine;

// 게임 매니저 클래스 (싱글톤 패턴 적용, 각 매니저 접근 제공)

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;                   // 싱글톤 인스턴스
    public static GameManager Instance => _instance;        // 싱글톤 인스턴스

    // 각 매니저 인터페이스 참조
    private IGameStateManager _stateManager;
    private IStageManager _stageManager;
    private IClueTracker _clueTracker;
    private IEndingManager _endingManager;

    private void Awake()
    {
        if (_instance != null && _instance != this)         // 이미 인스턴스가 존재하면 자신을 파괴
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeManagers();
    }

    // 매니저 인스턴스 초기화
    private void InitializeManagers()
    {
        _stateManager = new GameStateManager();
        _stageManager = new StageManager();
        _clueTracker = new ClueTracker();
        _endingManager = new EndingManager();
    }

    // 각 매니저에 대한 공개 접근자
    public IGameStateManager StateManager => _stateManager;
    public IStageManager StageManager => _stageManager;
    public IClueTracker ClueTracker => _clueTracker;
    public IEndingManager EndingManager => _endingManager;
}