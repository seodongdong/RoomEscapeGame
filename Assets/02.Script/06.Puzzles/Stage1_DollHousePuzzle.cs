using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 1스테이지: 인형의 집 퍼즐
/// 기획서: 장난감 의자, 장롱, 서랍, 인형 등을 알맞은 위치에 배치
/// ⭐ 잘못된 위치 → 대사 출력
/// </summary>
public class Stage1_DollHousePuzzle : PuzzleBase
{
	[System.Serializable]
	public class DollItem
	{
		public string itemId;              // 예: "toy_chair"
		public Transform targetSlot;       // 정확한 슬롯 위치
		public GameObject prefab;          // 아이템 프리팹
		public Vector3 correctPosition;    // 정확한 위치
	}

	[Header("Items")]
	[SerializeField] private List<DollItem> requiredItems;

	[Header("Feedback")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string wrongPositionDialogue = "여기가 아닌 것 같은데...";
	[TextArea(2, 5)]
	[SerializeField] private string correctPositionDialogue = "이 자리가 맞는 것 같아!";

	[Header("Tolerance")]
	[SerializeField] private float positionTolerance = 0.5f; // 허용 오차

	private Dictionary<string, bool> _placedItems = new Dictionary<string, bool>();

	private void Awake()
	{
		foreach (var item in requiredItems)
		{
			_placedItems[item.itemId] = false;
		}
	}

	/// <summary>
	/// 아이템 배치 시도
	/// 기획서: "실패 없음. 성공할때까지 계속 시도 가능"
	/// </summary>
	public bool PlaceItem(string itemId, Vector3 position)
	{
		var item = requiredItems.Find(i => i.itemId == itemId);
		if (item == null) return false;

		// 위치 체크
		float distance = Vector3.Distance(position, item.correctPosition);
		bool isCorrectPosition = distance <= positionTolerance;

		if (isCorrectPosition)
		{
			_placedItems[itemId] = true;
			ShowFeedback(correctPositionDialogue);
			CheckSolution();
			return true;
		}
		else
		{
			ShowFeedback(wrongPositionDialogue);
			return false;
		}
	}

	private void ShowFeedback(string message)
	{
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, message);
	}

	protected override bool IsSolutionCorrect()
	{
		foreach (var placed in _placedItems.Values)
		{
			if (!placed) return false;
		}
		return true;
	}

	protected override void SolvePuzzle()
	{
		base.SolvePuzzle();

		// 기획서: "잠긴 문이 열리는 소리"
		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("door_unlock");
	}

	private void OnDrawGizmos()
	{
		if (requiredItems == null) return;

		Gizmos.color = Color.green;
		foreach (var item in requiredItems)
		{
			Gizmos.DrawWireSphere(item.correctPosition, positionTolerance);
		}
	}
}