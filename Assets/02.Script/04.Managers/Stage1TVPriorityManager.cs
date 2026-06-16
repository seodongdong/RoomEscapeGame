using UnityEngine;

/// <summary>
/// Stage 1 전용 — TV 먼저 시청 요구 시스템
/// TV를 4회 시청하기 전까지 다른 오브젝트의 F키 상호작용을 차단한다.
///
/// [버그 수정]
/// - IsTVWatched를 Awake에서 항상 false로 초기화
///   → 씬 재진입 / 게임 재시작 시 true가 고정되던 버그 해결
/// - OnDestroy에서 static 상태도 함께 초기화
///   → Stage 2~5 씬에서 Instance == null 체크가 올바르게 작동
///
/// [사용법]
/// 각 Interactable.Interact() 첫 줄에 아래 코드 삽입:
///   if (Stage1TVPriorityManager.CheckPriorityBlocked(player)) return;
/// Stage 2~5는 Instance가 null이므로 자동으로 차단 안 됨.
/// </summary>
public class Stage1TVPriorityManager : MonoBehaviour
{
	private static Stage1TVPriorityManager _instance;
	public static Stage1TVPriorityManager Instance => _instance;

	[Header("Settings")]
	[SerializeField] private string priorityDialogue = "먼저 TV를 살펴보자...";
	[SerializeField] private string speaker = "소년";

	// ★ static 상태 — Awake와 OnDestroy 양쪽에서 명시적으로 초기화
	public static bool IsTVWatched { get; private set; } = false;

	private void Awake()
	{
		_instance = this;

		// ★ 씬 재진입 / 재시작 시 항상 false로 초기화
		IsTVWatched = false;
		Debug.Log("[Stage1TVPriority] 초기화 — IsTVWatched = false");
	}

	private void OnDestroy()
	{
		// ★ 씬 전환 시 static 상태도 함께 리셋
		if (_instance == this)
		{
			_instance = null;
			IsTVWatched = false;
			Debug.Log("[Stage1TVPriority] 파괴 — static 상태 초기화");
		}
	}

	/// <summary>
	/// TV 시청 완료 표시 (TVPlayer.cs의 4단계 완료 시 호출)
	/// </summary>
	public static void SetTVWatched()
	{
		IsTVWatched = true;
		Debug.Log("[Stage1TVPriority] TV 시청 완료 — 다른 상호작용 허용");
	}

	/// <summary>
	/// 각 Interactable.Interact() 첫 줄에서 호출.
	/// true 반환 → 차단 (TV 먼저 봐야 함)
	/// false 반환 → 통과 (상호작용 허용)
	///
	/// Instance == null이면 Stage 2~5 씬이므로 항상 false(허용).
	/// </summary>
	public static bool CheckPriorityBlocked(IPlayer player)
	{
		// Stage 2~5 등 이 매니저가 없는 씬 → 차단 안 함
		if (Instance == null) return false;

		// TV 이미 시청 완료 → 차단 안 함
		if (IsTVWatched) return false;

		// TV 먼저 보라는 안내 대사 출력
		var uiManager = UnityEngine.Object.FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(Instance.speaker, Instance.priorityDialogue);

		return true; // 차단
	}
}