using UnityEngine;

/// <summary>
/// 단서 추적 인터페이스
/// 수집한 단서 추적, 진엔딩 조건(15개) 확인용
/// </summary>
public interface IClueTracker
{
	void RegisterClue(string clueId);
	bool HasClue(string clueId);
	int GetClueCount();
	int GetTotalCluesInStage(int stage);
}