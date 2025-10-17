using UnityEngine;

// 퍼즐 인터페이스

public interface IPuzzle
{
    string PuzzleId { get; }
    bool IsSolved { get; }
    void StartPuzzle();
    void CheckSolution();
    event System.Action OnPuzzleSolved;     // 퍼즐이 해결되었을 때 발생하는 이벤트 
}
