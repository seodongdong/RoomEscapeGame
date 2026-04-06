using UnityEngine;
using System.Collections.Generic;

// 단서 수집 추적 매니저 클래스

public class ClueTracker : IClueTracker
{
    private HashSet<string> _collectedClues = new HashSet<string>();
    private Dictionary<int, int> _stageClueRequirements = new Dictionary<int, int>
    {
        { 1, 3 },
        { 2, 4 },
        { 3, 4 },
        { 4, 4 },   // 예시로 각 스테이지별 단서 개수 설정
        { 5, 0 }    // 엔딩 스테이지는 단서 없음
    };

    // 단서 등록
    public void RegisterClue(string clueId)
    {
        _collectedClues.Add(clueId);
        Debug.Log($"단서 등록: {clueId}. 총 단서 개수: {_collectedClues.Count}");
    }

    // 특정 단서가 수집되었는지 확인
    public bool HasClue(string clueId)
    {
        return _collectedClues.Contains(clueId);
    }

    // 수집된 단서의 총 개수 반환
    public int GetClueCount()
    {
        return _collectedClues.Count;
    }

    // 특정 스테이지에서 필요한 단서의 총 개수 반환
    public int GetTotalCluesInStage(int stage)
    {
        return _stageClueRequirements.ContainsKey(stage)
            ? _stageClueRequirements[stage]
            : 0;
    }
}
