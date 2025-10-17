using UnityEngine;

// 단서 수집 추적 인터페이스

public interface IClueTracker
{
    void RegisterClue(string clueId);
    bool HasClue(string clueId);
    int GetClueCount();
    int GetTotalCluesInStage(int stage);
}
