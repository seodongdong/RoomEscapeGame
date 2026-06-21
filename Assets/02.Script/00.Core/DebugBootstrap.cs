using UnityEngine;

/// <summary>
/// 개별 씬을 단독으로 테스트할 때 사용하는 디버그용 부트스트랩.
///
/// [용도]
/// 00_Boot를 거치지 않고 특정 스테이지 씬을 에디터에서 직접 열어
/// Play할 때, GameManager.Instance가 없어서 발생하는 여러 문제
/// (씬 전환 실패, 상태 전환 무시, NullReferenceException 등)를
/// 막기 위한 보조 스크립트입니다.
///
/// 씬에 GameManager가 이미 있으면(정상적으로 Boot를 거친 경우)
/// 아무 동작도 하지 않습니다. 즉 정식 플레이 흐름에는 전혀 영향을
/// 주지 않습니다.
///
/// [씬 배치]
/// 테스트하고 싶은 씬(Stage1~5 등)에 빈 GameObject로 배치하세요.
/// 다른 모든 스크립트보다 먼저 실행되어야 하므로, Project Settings →
/// Script Execution Order에서 이 스크립트를 가장 먼저로 설정하는 것을
/// 권장합니다(설정하지 않아도 대부분의 경우 Awake 시점에 큰 문제는
/// 없지만, UILayerManager 등이 GameManager.Instance를 참조하는
/// 시점과 경합할 수 있습니다).
///
/// [정식 빌드에서 제거 권장]
/// 이 스크립트는 테스트 편의용입니다. 정식 빌드/배포 전에는 각 씬에서
/// 제거하고, 00_Boot → BootSequence 흐름만 사용하는 것을 권장합니다.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class DebugBootstrap : MonoBehaviour
{
	[Header("디버그 전용 — 정식 빌드 전 제거 권장")]
	[SerializeField] private bool createGameManagerIfMissing = true;

	private void Awake()
	{
		if (!createGameManagerIfMissing) return;

		if (GameManager.Instance != null)
		{
			// 이미 Boot를 거쳐 정상적으로 존재함 — 아무것도 하지 않음
			Debug.Log("[DebugBootstrap] GameManager가 이미 존재합니다. 디버그 생성을 건너뜁니다.");
			return;
		}

		var go = new GameObject("GameManager (Debug Bootstrap)");
		go.AddComponent<GameManager>();

		Debug.LogWarning("[DebugBootstrap] GameManager.Instance가 없어 디버그용으로 새로 생성했습니다. " +
			"정식 플레이 흐름에서는 00_Boot 씬부터 시작하세요.");
	}
}