using UnityEngine;

/// <summary>
/// Stage 1 전용 TV 우선 시청 강제 시스템.
/// TV를 4회 시청하기 전까지 다른 모든 F키 상호작용을 차단합니다.
///
/// [핵심 수정 사항]
/// CheckPriorityBlocked()에 Instance == null 체크 추가.
/// Stage 2~5처럼 이 매니저가 씬에 없으면 절대 차단하지 않습니다.
/// Stage 1 씬에만 이 오브젝트를 배치하면 다른 스테이지엔 영향 없음.
/// </summary>
public class Stage1TVPriorityManager : MonoBehaviour
{
	private static Stage1TVPriorityManager _instance;
	public static Stage1TVPriorityManager Instance => _instance;

	[Header("Settings")]
	[SerializeField] private string priorityDialogue = "우선 TV를 살펴보자...";
	[SerializeField] private string speaker = "소년";

	// TV 시청 완료 여부 (static이라 씬 전환 후에도 유지될 수 있음)
	public static bool IsTVWatched { get; private set; } = false;

	private void Awake()
	{
		_instance = this;
		IsTVWatched = false; // 씬 진입 시 초기화
	}

	private void OnDestroy()
	{
		// 씬에서 제거될 때 Instance 정리
		// 다음 씬(Stage 2~5)에서 Instance가 남아있지 않도록
		if (_instance == this)
			_instance = null;
	}

	public static void SetTVWatched()
	{
		IsTVWatched = true;
		Debug.Log("[Stage1TVPriority] TV 시청 완료 → 다른 상호작용 해금");
	}

	/// <summary>
	/// 각 Interactable의 Interact() 맨 위에서 호출합니다.
	/// true 반환 시 → 상호작용 차단 (TV 먼저 보세요)
	/// false 반환 시 → 상호작용 허용
	///
	/// [수정] Instance == null이면 무조건 false(허용) 반환.
	///        Stage 2~5처럼 이 매니저가 없는 씬에서는 절대 차단하지 않습니다.
	/// </summary>
	public static bool CheckPriorityBlocked(IPlayer player)
	{
		// ★ 핵심 수정: 씬에 매니저가 없으면 차단하지 않음
		if (Instance == null) return false;

		// TV를 이미 봤으면 차단하지 않음
		if (IsTVWatched) return false;

		// TV 아직 안 봤음 → 안내 대사 출력 후 차단
		var uiManager = UnityEngine.Object.FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(Instance.speaker, Instance.priorityDialogue);

		return true; // 차단
	}
}