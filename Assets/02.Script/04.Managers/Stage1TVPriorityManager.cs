using UnityEngine;

/// <summary>
/// 1스테이지 TV 우선순위 관리
/// TV 4단계 완료 전까지 다른 오브젝트 상호작용 차단
/// </summary>
public class Stage1TVPriorityManager : MonoBehaviour
{
	private static Stage1TVPriorityManager _instance;
	public static Stage1TVPriorityManager Instance => _instance;

	[Header("Settings")]
	[SerializeField] private string priorityDialogue = "우선 TV를 살펴보자...";
	[SerializeField] private string speaker = "소년";

	// TV 4단계 완료 여부
	public static bool IsTVWatched { get; private set; } = false;

	private void Awake()
	{
		_instance = this;
		IsTVWatched = false; // 씬 로드마다 초기화
	}

	public static void SetTVWatched()
	{
		IsTVWatched = true;
		Debug.Log("[Stage1] TV 우선순위 해제");
	}

	/// <summary>
	/// 다른 오브젝트들의 Interact() 첫 줄에서 호출
	/// TV 미시청이면 대사 출력 후 true 반환 → 호출부에서 return
	/// </summary>
	public static bool CheckPriorityBlocked(IPlayer player)
	{
		if (IsTVWatched) return false; // 이미 봤으면 통과

		var uiManager = UnityEngine.Object.FindAnyObjectByType<UIManager>();
		if (Instance != null)
			uiManager?.ShowDialogue(Instance.speaker, Instance.priorityDialogue);

		return true; // 차단됨
	}
}