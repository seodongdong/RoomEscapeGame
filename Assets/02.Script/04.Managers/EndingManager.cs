using UnityEngine;

// 엔딩 매니저 클래스 (노말, 진엔딩, 게임오버 처리)

public class EndingManager : IEndingManager
{
    // 진엔딩에 필요한 전체 단서 개수
    private const int REQUIRED_CLUES_FOR_TRUE_ENDING = 15;

    public EndingType CheckEndingConditions(IInventory inventory, bool girlRescued)
    {
        // 노말엔딩: 소녀 구출 + 단서 부족
        if (girlRescued && !HasAllClues())
        {
            return EndingType.Normal;
        }

        // 진엔딩: 소녀 구출 + 모든 단서
        if (girlRescued && HasAllClues())
        {
            return EndingType.True;
        }

        // 여기 도달하면 안 됨 (추격전에서 게임오버 됐어야 함)
        Debug.LogError("엔딩 조건 오류: 잘못된 경로입니다!");
        return EndingType.Normal;
    }

    private bool HasAllClues()
    {
        int totalClues = GameManager.Instance.ClueTracker.GetClueCount();
        return totalClues >= REQUIRED_CLUES_FOR_TRUE_ENDING;
    }

    public void TriggerEnding(EndingType endingType)
    {
        Debug.Log($"엔딩 발동: {endingType}");
        
        switch (endingType)
        {
            case EndingType.GameOver:
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
                break;
                
            case EndingType.Normal:
                UnityEngine.SceneManagement.SceneManager.LoadScene("NormalEndingScene");
                break;
                
            case EndingType.True:
                UnityEngine.SceneManagement.SceneManager.LoadScene("TrueEndingScene");
                break;
        }
    }
}