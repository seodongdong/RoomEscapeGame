using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 단서 데이터베이스 (ScriptableObject)
/// 모든 단서를 중앙에서 관리
/// </summary>
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
		public bool isKeyClue; // 중요한 스토리 단서
	}

	[SerializeField] private List<ClueData> allClues = new List<ClueData>();

	/// <summary>
	/// ID로 단서 찾기
	/// </summary>
	public ClueData GetClue(string clueId)
	{
		return allClues.Find(c => c.clueId == clueId);
	}

	/// <summary>
	/// 스테이지별 단서 목록
	/// </summary>
	public List<ClueData> GetCluesByStage(int stage)
	{
		return allClues.FindAll(c => c.stageNumber == stage);
	}

	/// <summary>
	/// 전체 단서 개수
	/// </summary>
	public int GetTotalClueCount()
	{
		return allClues.Count;
	}

	/// <summary>
	/// 스테이지별 단서 개수
	/// </summary>
	public int GetStageClueCount(int stage)
	{
		return allClues.FindAll(c => c.stageNumber == stage).Count;
	}

	/// <summary>
	/// 핵심 단서 목록
	/// </summary>
	public List<ClueData> GetKeyClues()
	{
		return allClues.FindAll(c => c.isKeyClue);
	}

#if UNITY_EDITOR
	[ContextMenu("Add Sample Clues")]
	private void AddSampleClues()
	{
		allClues.Clear();

		// 1스테이지 샘플
		allClues.Add(new ClueData
		{
			clueId = "diary_page_1",
			clueName = "찢어진 일기장",
			description = "누군가 그린 그림 일기. 스파게티처럼 얽힌 선들...",
			stageNumber = 1,
			speaker = "소년",
			dialogue = "찢어진 일기장이다... 누가 그린 걸까?",
			isKeyClue = true
		});

		allClues.Add(new ClueData
		{
			clueId = "hanbok_gift",
			clueName = "새삥 한복",
			description = "깨끗한 한복. 이름 자수가 있다.",
			stageNumber = 1,
			speaker = "소년",
			dialogue = "한복에 이름이 적혀있다...",
			isKeyClue = true
		});

		Debug.Log("샘플 단서 추가 완료!");
	}
#endif
}