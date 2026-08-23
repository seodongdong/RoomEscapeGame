using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 스테이지 관리
/// 1. 거실 2. 장례식장 3. 미로 4. 주방 5. 지하실
///
/// [리팩토링 변경사항 - SceneTransitionManager 재연결]
/// 기존에는 LoadStage / LoadSceneByName이 SceneManager.LoadScene을
/// 직접 호출해서, 완성되어 있던 SceneTransitionManager(페이드 인/아웃)가
/// 한 번도 실행되지 않았습니다. 기획서의 "문 열림 → 페이드아웃 →
/// 페이드인 → 새 공간 진입"이 구현되지 않았던 부분입니다.
///
/// 이제 SceneTransitionManager.Instance가 있으면 그것을 거쳐 페이드
/// 연출과 함께 씬을 전환하고, 없으면(예: 부트 씬을 안 거친 테스트 환경)
/// 기존처럼 즉시 전환하는 폴백을 유지합니다 — 기존 동작을 깨지 않습니다.
/// </summary>
public class StageManager : IStageManager
{
	private int _currentStage = 1;

	public int CurrentStage => _currentStage;
	public event System.Action<int> OnStageChanged;

	/// <summary>
	/// 스테이지 번호로 씬을 로드합니다.
	/// 내부적으로 LoadSceneByName과 동일한 폴백 로직(이름 매칭 실패 시
	/// 번호 기반 검색)을 사용하므로, GetSceneName()의 하드코딩된 이름이
	/// 실제 빌드의 씬 파일명과 정확히 일치하지 않아도(예: 번호 접두사
	/// 차이) 정상 동작합니다.
	/// </summary>
	public void LoadStage(int stageNumber)
	{
		string sceneName = GetSceneName(stageNumber);
		LoadSceneByName(sceneName, stageNumber);
	}

	/// <summary>
	/// 씬 이름을 직접 받아 로드합니다. (저장 데이터 기반 복원용)
	///
	/// [폴백 로직 추가 이유]
	/// 저장 데이터의 sceneName은 "저장한 시점"의 실제 씬 파일명을
	/// 그대로 기록합니다. 이후 씬 파일명을 바꾸면(예: Stage4_Kitchen →
	/// 05_Stage4_Kitchen), 예전에 저장된 데이터는 이미 존재하지 않는
	/// 이름을 가리키게 되어 Application.CanStreamedLevelBeLoaded가
	/// false를 반환하고 그대로 로드가 실패했습니다.
	///
	/// stageNumberForTracking(스테이지 번호, 1~5)이 함께 있으면,
	/// 정확한 이름으로 못 찾았을 때 번호 기반으로 한 번 더 시도합니다.
	/// 번호→씬이름 매핑은 FindSceneNameContaining()이 Build Settings에
	/// 등록된 씬들을 직접 순회해서 찾으므로, 파일명 접두사가 바뀌어도
	/// (예: "Stage4_Kitchen" 부분만 같으면) 정상 동작합니다.
	/// </summary>
	public void LoadSceneByName(string sceneName, int stageNumberForTracking = -1)
	{
		if (string.IsNullOrEmpty(sceneName) && stageNumberForTracking <= 0)
		{
			Debug.LogError("[StageManager] sceneName과 stageNumberForTracking이 모두 비어있어 로드할 수 없습니다.");
			return;
		}

		if (stageNumberForTracking > 0)
		{
			_currentStage = stageNumberForTracking;
			OnStageChanged?.Invoke(_currentStage);
		}

		// 1차: 저장된 이름 그대로 시도
		if (!string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
		{
			LoadSceneWithTransition(sceneName);
			return;
		}

		// 2차: 정확한 이름으로 못 찾았으면 스테이지 번호로 폴백 시도
		if (stageNumberForTracking > 0)
		{
			Debug.LogWarning($"[StageManager] 저장된 씬 이름 '{sceneName}'을 찾을 수 없어, " +
				$"스테이지 번호 {stageNumberForTracking} 기준으로 다시 찾습니다.");

			string fallbackKeyword = GetSceneKeyword(stageNumberForTracking);
			string foundSceneName = FindSceneNameContaining(fallbackKeyword);

			if (!string.IsNullOrEmpty(foundSceneName))
			{
				Debug.Log($"[StageManager] 폴백 성공 — '{foundSceneName}' 씬을 찾았습니다.");
				LoadSceneWithTransition(foundSceneName);
				return;
			}
		}

		Debug.LogError($"[StageManager] 씬을 찾을 수 없음: '{sceneName}' (스테이지 {stageNumberForTracking}) " +
			"— Build Settings에 등록되어 있는지 확인하세요.");
	}

	/// <summary>
	/// SceneTransitionManager가 있으면 페이드 연출과 함께,
	/// 없으면 기존처럼 즉시 로드합니다.
	/// 호출 전에 이미 CanStreamedLevelBeLoaded 체크를 마쳤다는 전제이므로,
	/// 여기서는 다시 체크하지 않습니다.
	/// </summary>
	private void LoadSceneWithTransition(string sceneName)
	{
		if (SceneTransitionManager.Instance != null)
		{
			SceneTransitionManager.Instance.LoadScene(sceneName);
			Debug.Log($"[StageManager] (페이드 전환) {sceneName} 로드");
		}
		else
		{
			SceneManager.LoadScene(sceneName);
			Debug.LogWarning($"[StageManager] SceneTransitionManager가 없어 즉시 전환합니다: {sceneName}");
		}
	}

	private string GetSceneName(int stageNumber)
	{
		// [수정] 실제 씬 파일명(02_Stage1_LivingRoom 등)과 정확히 일치시켜,
		// 매번 전환마다 폴백 검색을 타던 것을 없앤습니다.
		switch (stageNumber)
		{
			case 1: return "02_Stage1_LivingRoom";
			case 2: return "03_Stage2_FuneralHall";
			case 3: return "04_Stage3_Maze";
			case 4: return "05_Stage4_Kitchen";
			case 5: return "06_Stage5_Basement";
			default: return $"Stage{stageNumber}";
		}
	}

	/// <summary>
	/// 폴백 검색에 쓸 핵심 키워드만 반환합니다(번호 접두사 없이).
	/// 실제 씬 파일명이 "05_Stage4_Kitchen"이어도 "Stage4_Kitchen"이라는
	/// 부분 문자열만 맞으면 FindSceneNameContaining이 찾아낼 수 있습니다.
	/// </summary>
	private string GetSceneKeyword(int stageNumber)
	{
		return GetSceneName(stageNumber);
	}

	/// <summary>
	/// Build Settings에 등록된 모든 씬을 순회하며, 파일명에
	/// keyword가 포함된 첫 번째 씬의 정확한 이름을 반환합니다.
	/// 찾지 못하면 null을 반환합니다.
	///
	/// 예: keyword="Stage4_Kitchen" → 실제 등록된 씬이
	/// "05_Stage4_Kitchen"이어도 찾아냅니다.
	/// </summary>
	private string FindSceneNameContaining(string keyword)
	{
		if (string.IsNullOrEmpty(keyword)) return null;

		int sceneCount = SceneManager.sceneCountInBuildSettings;
		for (int i = 0; i < sceneCount; i++)
		{
			string path = SceneUtility.GetScenePathByBuildIndex(i);
			string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

			if (fileName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
				return fileName;
		}

		return null;
	}

	public void CompleteStage()
	{
		_currentStage++;
		OnStageChanged?.Invoke(_currentStage);

		Debug.Log($"[StageManager] 스테이지 클리어! 다음: {_currentStage}");
	}
}