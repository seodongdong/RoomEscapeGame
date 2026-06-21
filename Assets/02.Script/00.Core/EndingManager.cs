using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 관리
/// 기획서: 게임오버 / 노말(캠코더X) / 진엔딩(캠코더O)
///
/// [리팩토링 변경사항]
/// 생성자에서 IClueTracker를 주입받도록 변경했습니다.
/// 기존에는 ClueTracker가 "15개 단서 수집"을 추적하고 있었지만
/// 실제 엔딩 판정(CheckEndingConditions)은 이를 전혀 참조하지 않아
/// 두 시스템이 서로 다른 진실을 가진 상태였습니다.
///
/// 이번 변경은 "연결 통로"만 만든 것입니다.
/// CheckEndingConditions의 실제 판정 로직(소녀 구출 + 캠코더 보유)은
/// 기획서 그대로 유지했습니다 — 단서 개수를 엔딩 조건에 반영할지는
/// 기획 결정이 필요한 영역이라 코드만으로 임의로 바꾸지 않았습니다.
/// 필요해지면 _clueTracker.GetClueCount()를 가져와 조건에 추가하면 됩니다.
/// </summary>
public class EndingManager : IEndingManager
{
	private readonly IClueTracker _clueTracker;

	public EndingManager(IClueTracker clueTracker)
	{
		_clueTracker = clueTracker;
	}

	public EndingType CheckEndingConditions(IInventory inventory, bool girlRescued, bool hasCamcorder)
	{
		// 참고용 로그 — ClueTracker가 실제로 연결되어 있음을 확인할 수 있습니다.
		int clueCount = _clueTracker?.GetClueCount() ?? -1;
		Debug.Log($"[EndingManager] 판정 시점 — 수집 단서: {clueCount}개, 소녀구출: {girlRescued}, 캠코더: {hasCamcorder}");

		// 소녀 구출 실패 → 게임오버
		if (!girlRescued)
		{
			return EndingType.GameOver;
		}

		// 소녀 구출 성공 + 캠코더 미수집 → 노말 엔딩
		if (!hasCamcorder)
		{
			Debug.Log("[EndingManager] 노말 엔딩 (캠코더 미수집)");
			return EndingType.Normal;
		}

		// 소녀 구출 성공 + 캠코더 수집 → 진엔딩
		Debug.Log("[EndingManager] 진엔딩 (캠코더 수집 완료)");
		return EndingType.True;
	}

	public void TriggerEnding(EndingType endingType)
	{
		Debug.Log($"[EndingManager] 엔딩 발동: {endingType}");

		switch (endingType)
		{
			case EndingType.GameOver:
				SceneManager.LoadScene("GameOverScene");
				break;

			case EndingType.Normal:
				SceneManager.LoadScene("NormalEndingScene");
				break;

			case EndingType.True:
				SceneManager.LoadScene("TrueEndingScene");
				break;
		}
	}
}
