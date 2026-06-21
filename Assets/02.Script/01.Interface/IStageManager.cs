using UnityEngine;

/// <summary>
/// 스테이지 관리 인터페이스
/// 1~5 스테이지 로드 및 진행 관리
///
/// [추가]
/// LoadSceneByName — 저장 데이터에 기록된 실제 씬 이름으로 직접 로드.
/// 번호 → 이름 매핑(LoadStage)을 거치지 않아 매핑 불일치 문제를 회피합니다.
/// </summary>
public interface IStageManager
{
	int CurrentStage { get; }

	void LoadStage(int stageNumber);
	void CompleteStage();

	// ★ 추가
	void LoadSceneByName(string sceneName, int stageNumberForTracking = -1);

	event System.Action<int> OnStageChanged;
}