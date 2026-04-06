using UnityEngine;

/// <summary>
/// 5스테이지 추격전
/// 기획서: 2분 제한, 범인 추격, 소녀 구출, 엔딩 분기
/// </summary>
public class ChaseSequence : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Player player;
	[SerializeField] private Girl girl;
	[SerializeField] private EnemyChaser criminal;
	[SerializeField] private Transform exitDoor;

	[Header("Chase Settings")]
	[SerializeField] private float chaseDuration = 120f; // 2분
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

		GameManager.Instance.StateManager.ChangeState(GameState.Chase);

		_uiManager = FindAnyObjectByType<UIManager>();
		_uiManager.StartTimer(chaseDuration);

		criminal.Chase(player.transform);

		var audioManager = FindAnyObjectByType<AudioManager>();
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

		Debug.Log($"[Chase] 게임오버: {reason}");

		GameManager.Instance.StateManager.ChangeState(GameState.GameOver);
		GameManager.Instance.EndingManager.TriggerEnding(EndingType.GameOver);
	}
}