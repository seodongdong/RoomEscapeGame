using UnityEngine;

/// <summary>
/// 개별 씬 단독 테스트용 — 항상 활성 상태로 씬에 배치하세요.
/// GameManager가 이미 있으면 아무것도 하지 않습니다.
/// 정식 플레이(00_Boot 경유)에는 영향 없습니다.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class DebugBootstrap : MonoBehaviour
{
	[Header("디버그 전용")]
	[SerializeField] private bool createGameManagerIfMissing = true;
	[SerializeField] private bool logStatus = true;

	private void Awake()
	{
		if (!createGameManagerIfMissing) return;

		if (GameManager.Instance != null)
		{
			if (logStatus)
				Debug.Log("[DebugBootstrap] GameManager 이미 존재 — 건너뜀");
			return;
		}

		var go = new GameObject("GameManager [Debug]");
		go.AddComponent<GameManager>();
		Debug.LogWarning("[DebugBootstrap] GameManager 없음 → 디버그용 생성. 정식 플레이는 00_Boot부터 시작하세요.");
	}
}