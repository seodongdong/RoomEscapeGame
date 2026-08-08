using System.Collections.Generic;

/// <summary>
/// 단서 추적 인터페이스
/// 수집한 단서 추적, 진엔딩 조건 확인용
///
/// [v2 변경사항]
/// GetAllClues() / RestoreClues() 추가.
/// 배치용 단서(RegisterClueOnly)는 인벤토리에 들어가지 않기 때문에,
/// 저장할 때 ClueTracker에서 직접 목록을 꺼내야 합니다.
/// RestoreClues는 불러오기 시 이전 세션의 단서를 지우고 덮어씁니다.
/// </summary>
public interface IClueTracker
{
	void RegisterClue(string clueId);
	bool HasClue(string clueId);
	int GetClueCount();
	int GetTotalCluesInStage(int stage);

	/// <summary>저장용 — 수집한 모든 단서 ID</summary>
	List<string> GetAllClues();

	/// <summary>불러오기용 — 기존 목록을 비우고 저장된 목록으로 교체</summary>
	void RestoreClues(List<string> clueIds);
}