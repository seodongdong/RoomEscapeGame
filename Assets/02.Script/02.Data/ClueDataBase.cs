using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ClueDatabase", menuName = "Game/Clue Database")]
public class ClueDatabase : ScriptableObject
{
	[System.Serializable]
	public class ClueData
	{
		[Header("Basic Info")]
		public string clueId;
		public string clueName;
		[TextArea(3, 10)]
		public string description;
		public Sprite icon;

		[Header("Stage Info")]
		public int stageNumber;

		[Header("Dialogue")]
		public string speaker = "소년";
		[TextArea(2, 5)]
		public string dialogue;

		[Header("Special")]
		public bool isKeyClue; // 중요한 스토리 단서 (⭐ 표시용)
	}

	[SerializeField] private List<ClueData> allClues = new List<ClueData>();

	// ID로 단서 찾기
	public ClueData GetClue(string clueId)
	{
		return allClues.Find(c => c.clueId == clueId);
	}

	// 스테이지별 단서 목록
	public List<ClueData> GetCluesByStage(int stage)
	{
		return allClues.FindAll(c => c.stageNumber == stage);
	}

	// 전체 단서 개수
	public int GetTotalClueCount()
	{
		return allClues.Count;
	}

	// 스테이지별 단서 개수
	public int GetStageClueCount(int stage)
	{
		return allClues.FindAll(c => c.stageNumber == stage).Count;
	}

	// 핵심 단서 목록
	public List<ClueData> GetKeyClues()
	{
		return allClues.FindAll(c => c.isKeyClue);
	}
}