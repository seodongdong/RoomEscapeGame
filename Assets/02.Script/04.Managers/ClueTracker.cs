using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 단서 추적
/// 기획서: 1스테이지 3개, 2~4스테이지 각 4개 = 총 15개
/// </summary>
public class ClueTracker : IClueTracker
{
	private HashSet<string> _collectedClues = new HashSet<string>();

	private Dictionary<int, int> _stageClueRequirements = new Dictionary<int, int>
	{
		{ 1, 3 },  // 거실
        { 2, 4 },  // 장례식장
        { 3, 4 },  // 미로
        { 4, 4 },  // 주방
        { 5, 0 }   // 지하실 (추격전)
    };

	public void RegisterClue(string clueId)
	{
		_collectedClues.Add(clueId);
		Debug.Log($"[ClueTracker] 단서 등록: {clueId} (총 {_collectedClues.Count}/15개)");
	}

	public bool HasClue(string clueId)
	{
		return _collectedClues.Contains(clueId);
	}

	public int GetClueCount()
	{
		return _collectedClues.Count;
	}

	public int GetTotalCluesInStage(int stage)
	{
		return _stageClueRequirements.ContainsKey(stage)
			? _stageClueRequirements[stage]
			: 0;
	}
}