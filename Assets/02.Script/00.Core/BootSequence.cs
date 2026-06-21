using UnityEngine;

/// <summary>
/// 00_Boot 씬 전용 — 게임 시작 시 1회 실행되어 메인메뉴로 자동 전환합니다.
///
/// [추가 이유]
/// GameManager와 SceneTransitionManager는 DontDestroyOnLoad로
/// 게임 전체에서 1개만 유지되도록 설계되어 있는데, 00_Boot 씬 자체에는
/// "다음 씬으로 넘어가라"는 로직이 없었습니다. 그래서 00_Boot를 직접
/// Play하면 그 자리에 멈춰있고, 결국 01_MainMenu 씬을 에디터에서
/// 직접 열어 Play하는 습관이 생기는데, 이 경우 GameManager가 전혀
/// 생성되지 않아 SaveSlotUI 등에서 NullReferenceException이 발생합니다.
///
/// 이 스크립트는 그 문제를 막기 위해 Boot 씬이 항상 "GameManager를
/// 만든 다음, 곧바로 메인메뉴로 넘어간다"는 흐름을 보장합니다.
///
/// [씬 배치]
/// 00_Boot 씬에 빈 GameObject로 하나만 배치하세요(GameManager와
/// 같은 오브젝트에 붙여도 무방합니다).
/// </summary>
public class BootSequence : MonoBehaviour
{
	[Header("다음으로 이동할 씬 이름")]
	[SerializeField] private string mainMenuSceneName = "01_MainMenu";

	[Header("전환 전 대기 시간(초) — 0이면 즉시 전환")]
	[SerializeField] private float delayBeforeLoad = 0f;

	private void Start()
	{
		if (delayBeforeLoad > 0f)
			Invoke(nameof(GoToMainMenu), delayBeforeLoad);
		else
			GoToMainMenu();
	}

	private void GoToMainMenu()
	{
		if (string.IsNullOrEmpty(mainMenuSceneName))
		{
			Debug.LogError("[BootSequence] mainMenuSceneName이 비어있습니다.");
			return;
		}

		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
			Debug.Log($"[BootSequence] (페이드 전환) {mainMenuSceneName} 로드");
		}
		else
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
			Debug.LogWarning($"[BootSequence] SceneTransitionManager가 없어 즉시 전환합니다: {mainMenuSceneName}");
		}
	}
}