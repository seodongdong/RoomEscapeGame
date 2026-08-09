using UnityEngine;

/// <summary>
/// 07_Ending 씬 전용 컨트롤러.
///
/// EndingManager.TriggerEnding()이 GameManager에 남겨둔 EndingType을 꺼내서
/// 세 가지 결과(게임오버 / 노말엔딩 / 진엔딩) 중 알맞은 패널을 켭니다.
///
/// [현재 구현]
/// 컷씬 재생 시스템(7장, Phase 5)이 아직 없으므로 우선 패널 On/Off로 분기만 합니다.
/// 이후 CutscenePlayer가 만들어지면 각 case 안에서
/// CutscenePlayer.Instance.Play(CS-06/07/08) 호출로 교체하면 됩니다.
///
/// [씬 설정]
/// 07_Ending 씬에 이 스크립트를 빈 GameObject(예: "EndingSceneController")에 부착하고,
/// 아래 세 패널을 Inspector에 연결하세요. 세 패널은 모두 처음엔 비활성 상태로 두면 됩니다.
/// </summary>
public class EndingSceneController : MonoBehaviour
{
	[Header("엔딩별 패널 (모두 초기 비활성 상태로 배치)")]
	[SerializeField] private GameObject gameOverPanel;
	[SerializeField] private GameObject normalEndingPanel;
	[SerializeField] private GameObject trueEndingPanel;

	private void Start()
	{
		if (GameManager.Instance == null)
		{
			Debug.LogError("[EndingSceneController] GameManager.Instance가 없습니다.");
			return;
		}

		EndingType endingType = GameManager.Instance.ConsumePendingEndingType();
		Debug.Log($"[EndingSceneController] 진입한 엔딩: {endingType}");

		gameOverPanel?.SetActive(false);
		normalEndingPanel?.SetActive(false);
		trueEndingPanel?.SetActive(false);

		switch (endingType)
		{
			case EndingType.GameOver:
				gameOverPanel?.SetActive(true);
				break;

			case EndingType.Normal:
				normalEndingPanel?.SetActive(true);
				break;

			case EndingType.True:
				trueEndingPanel?.SetActive(true);
				break;
		}
	}
}