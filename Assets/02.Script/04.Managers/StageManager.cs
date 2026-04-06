using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 스테이지 관리
/// 1. 거실 2. 장례식장 3. 미로 4. 주방 5. 지하실
/// </summary>
public class StageManager : IStageManager
{
	private int _currentStage = 1;

	public int CurrentStage => _currentStage;
	public event System.Action<int> OnStageChanged;

	public void LoadStage(int stageNumber)
	{
		_currentStage = stageNumber;
		OnStageChanged?.Invoke(_currentStage);

		string sceneName = GetSceneName(stageNumber);

		if (Application.CanStreamedLevelBeLoaded(sceneName))
		{
			SceneManager.LoadScene(sceneName);
			Debug.Log($"[StageManager] {sceneName} 로드");
		}
		else
		{
			Debug.LogWarning($"[StageManager] 씬을 찾을 수 없음: {sceneName}");
		}
	}

	private string GetSceneName(int stageNumber)
	{
		switch (stageNumber)
		{
			case 1: return "Stage1_LivingRoom";      // 거실
			case 2: return "Stage2_FuneralHall";     // 장례식장
			case 3: return "Stage3_Maze";            // 미로
			case 4: return "Stage4_Kitchen";         // 주방
			case 5: return "Stage5_Basement";        // 지하실
			default: return $"Stage{stageNumber}";
		}
	}

	public void CompleteStage()
	{
		_currentStage++;
		OnStageChanged?.Invoke(_currentStage);

		Debug.Log($"[StageManager] 스테이지 클리어! 다음: {_currentStage}");
	}
}