using UnityEngine;

/// <summary>
/// 퍼즐 인터페이스
/// 모든 퍼즐이 따라야 할 기본 구조 (시작/검증/해결)
/// </summary>
public interface IPuzzle
{
	string PuzzleId { get; }
	bool IsSolved { get; }

	void StartPuzzle();
	void CheckSolution();

	event System.Action OnPuzzleSolved;
}