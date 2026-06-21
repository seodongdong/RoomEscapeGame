using UnityEngine;

public class ChaseSequence : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Player player;
	[SerializeField] private Girl girl;
	[SerializeField] private EnemyChaser criminal;
	[SerializeField] private Transform exitDoor;

	[Header("Chase Settings")]
	[SerializeField] private float chaseDuration = 60f;
	[SerializeField] private float catchDistance = 1.5f;
	[SerializeField] private float girlRescueDistance = 2f;

	private bool _isChaseActive;
	private float _chaseTimer;
	private IUIManager _uiManager;
	private bool _girlRescued;

	public bool IsGirlRescued => _girlRescued;

	public void StartChase()
	{
		_isChaseActive = true;
		_chaseTimer = chaseDuration;
		_girlRescued = false;

		GameManager.Instance.ChangeState(GameState.Chase);

		_uiManager = GameServices.UI;
		_uiManager.StartTimer(chaseDuration);

		criminal.Chase(player.transform);

		var audioManager = GameServices.Audio;
		audioManager?.PlayBGM("chase_bgm");

		Debug.Log("[Chase] 추격전 시작!");
	}

	private void Update()
	{
		if (!_isChaseActive) return;

		_chaseTimer -= Time.deltaTime;

		if (_chaseTimer <= 0)
		{
			TriggerGameOver("시간 초과!");
			return;
		}

		float distanceToCriminal = Vector3.Distance(player.transform.position, criminal.transform.position);
		if (distanceToCriminal <= catchDistance)
		{
			TriggerGameOver("범인에게 잡혔습니다!");
			return;
		}

		CheckGirlRescue();
	}

	private void CheckGirlRescue()
	{
		if (_girlRescued || girl == null) return;

		float distanceToGirl = Vector3.Distance(player.transform.position, girl.transform.position);
		if (distanceToGirl <= girlRescueDistance)
		{
			_girlRescued = true;
			girl.StartFollowing();

			Debug.Log("[Chase] 소녀 구출 성공!");
		}
	}

	private void TriggerGameOver(string reason)
	{
		_isChaseActive = false;
		criminal.StopChasing();
		_uiManager.StopTimer();

		girl?.StopFollowing();

		Debug.Log($"[Chase] 게임오버: {reason} → 추격전 자동 저장 지점에서 재시작");

		GameManager.Instance.ChangeState(GameState.GameOver);

		RestartFromAutosave();
	}

	/// <summary>
	/// ★ 수정: 추격전 자동 저장 데이터의 sceneName으로 직접 재로드합니다.
	/// (StageManager의 번호 → 이름 매핑을 거치지 않아 매핑 불일치 문제 없음)
	/// </summary>
	private void RestartFromAutosave()
	{
		var saveSystem = FindAnyObjectByType<SaveSystem>();

		if (saveSystem != null && saveSystem.HasChaseAutosave())
		{
			GameData data = saveSystem.LoadChaseAutosave();

			if (data != null && !string.IsNullOrEmpty(data.sceneName))
			{
				// ★ 추가: 자동 저장 시점의 위치/인벤토리도 동일하게 복원
				GameManager.Instance?.SetPendingLoadData(data);

				GameManager.Instance.StageManager.LoadSceneByName(data.sceneName, data.currentStage);
				Debug.Log($"[ChaseSequence] 추격전 자동 저장 지점(씬: {data.sceneName})으로 재시작");
				return;
			}
		}

		Debug.LogWarning("[ChaseSequence] 추격전 자동 저장 데이터가 없어 현재 씬을 다시 로드합니다.");
		string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		GameManager.Instance.StageManager.LoadSceneByName(currentScene);
	}
}