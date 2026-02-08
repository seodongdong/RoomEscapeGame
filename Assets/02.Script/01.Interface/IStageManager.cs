using UnityEngine;

/// <summary>
/// 스테이지 관리 인터페이스
/// 1~5 스테이지 로드 및 진행 관리
/// </summary>
public interface IStageManager
{
	int CurrentStage { get; }

	void LoadStage(int stageNumber);
	void CompleteStage();

	event System.Action<int> OnStageChanged;
}