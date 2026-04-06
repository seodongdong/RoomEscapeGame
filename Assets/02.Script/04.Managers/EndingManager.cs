using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 관리
/// 기획서: 게임오버 / 노말(캠코더X) / 진엔딩(캠코더O)
/// </summary>
public class EndingManager : IEndingManager
{
	public EndingType CheckEndingConditions(IInventory inventory, bool girlRescued, bool hasCamcorder)
	{
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