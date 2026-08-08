using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 단서 추적
/// 기획서: 1스테이지 3개, 2~4스테이지 각 4개 = 총 15개
///
/// [v2 변경사항]
/// GetAllClues() / RestoreClues() 추가.
/// GameManager는 DontDestroyOnLoad라 이 객체가 세션 내내 살아 있습니다.
/// 그래서 불러오기 때 명시적으로 비워주지 않으면 이전 플레이의 단서가
/// 그대로 남아 진엔딩 판정까지 오염됩니다.
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
		if (string.IsNullOrEmpty(clueId)) return;

		_collectedClues.Add(clueId);
		Debug.Log($"[ClueTracker] 단서 등록: {clueId} (총 {_collectedClues.Count}/15개)");
	}

	public bool HasClue(string clueId) => _collectedClues.Contains(clueId);

	public int GetClueCount() => _collectedClues.Count;

	public int GetTotalCluesInStage(int stage)
		=> _stageClueRequirements.ContainsKey(stage) ? _stageClueRequirements[stage] : 0;

	public List<string> GetAllClues() => new List<string>(_collectedClues);

	public void RestoreClues(List<string> clueIds)
	{
		_collectedClues.Clear();

		if (clueIds != null)
		{
			foreach (var id in clueIds)
				if (!string.IsNullOrEmpty(id)) _collectedClues.Add(id);
		}

		Debug.Log($"[ClueTracker] 단서 복원: {_collectedClues.Count}개");
	}
}