using UnityEngine;

/// <summary>
/// Stage 1 전용 — TV 먼저 시청 요구 시스템
/// TV를 4회 시청하기 전까지 다른 오브젝트의 F키 상호작용을 차단한다.
///
/// [리팩토링 - 이름 변경]
/// 기존 Stage1TVGate에서 Stage1TVGate로 이름을 바꿨습니다.
/// "Manager"라는 이름이 과장되어 있었습니다 — 이 스크립트는 단일
/// 스테이지(1스테이지)에서만 동작하는 인터랙션 게이트 역할이지,
/// 다른 매니저들처럼 전역적인 책임을 갖지 않습니다.
/// InteractableBase가 모든 하위 클래스에서 이 게이트를 자동으로
/// 호출하므로, 더 이상 각 단서 스크립트마다 직접 호출 줄을 넣지
/// 않아도 됩니다.
///
/// [버그 수정]
/// - IsTVWatched를 Awake에서 항상 false로 초기화
///   → 씬 재진입 / 게임 재시작 시 true가 고정되던 버그 해결
/// - OnDestroy에서 static 상태도 함께 초기화
///   → Stage 2~5 씬에서 Instance == null 체크가 올바르게 작동
/// </summary>
public class Stage1TVGate : MonoBehaviour
{
	private static Stage1TVGate _instance;
	public static Stage1TVGate Instance => _instance;

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
		Debug.Log("[Stage1TVGate] 초기화 — IsTVWatched = false");
	}

	private void OnDestroy()
	{
		// ★ 씬 전환 시 static 상태도 함께 리셋
		if (_instance == this)
		{
			_instance = null;
			IsTVWatched = false;
			Debug.Log("[Stage1TVGate] 파괴 — static 상태 초기화");
		}
	}

	/// <summary>
	/// TV 시청 완료 표시 (TVPlayer.cs의 4단계 완료 시 호출)
	/// </summary>
	public static void SetTVWatched()
	{
		IsTVWatched = true;
		Debug.Log("[Stage1TVGate] TV 시청 완료 — 다른 상호작용 허용");
	}

	/// <summary>
	/// ★ 추가: 저장 데이터에서 TV 시청 여부를 복원합니다.
	/// Awake가 무조건 false로 초기화하기 때문에, 복원 없이는
	/// 불러오기 후 거실의 모든 상호작용이 다시 막힙니다.
	/// TVPlayer.LoadState에서 호출됩니다.
	/// </summary>
	public static void RestoreTVWatched(bool watched)
	{
		IsTVWatched = watched;
		Debug.Log($"[Stage1TVGate] 복원 — IsTVWatched = {watched}");
	}

	/// <summary>
	/// InteractableBase.Interact()에서 자동으로 호출됩니다.
	/// true 반환 → 차단 (TV 먼저 봐야 함)
	/// false 반환 → 통과 (상호작용 허용)
	///
	/// Instance == null이면 Stage 2~5 씬이므로 항상 false(허용).
	/// </summary>
	public static bool CheckPriorityBlocked(IPlayer player)
	{
		// Stage 2~5 등 이 게이트가 없는 씬 → 차단 안 함
		if (Instance == null) return false;

		// TV 이미 시청 완료 → 차단 안 함
		if (IsTVWatched) return false;

		// TV 먼저 보라는 안내 대사 출력
		var uiManager = GameServices.UI;
		uiManager?.ShowDialogue(Instance.speaker, Instance.priorityDialogue);

		return true; // 차단
	}
}