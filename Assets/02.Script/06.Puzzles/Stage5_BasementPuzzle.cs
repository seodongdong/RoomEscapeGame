using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 5스테이지: 지하실 퍼즐
/// 기획서: "장식장에 목각인형을 위치에 맞게 배열하고 열쇠 획득"
/// </summary>
public class Stage5_BasementPuzzle : PuzzleBase
{
	[System.Serializable]
	public class WoodenDollSlot
	{
		public string requiredDollId;   // "wooden_doll_1"
		public Transform slotPosition;
		public bool isPlaced;
	}

	[Header("Wooden Doll Slots")]
	[SerializeField] private List<WoodenDollSlot> dollSlots; // 4개

	[Header("Reward")]
	[SerializeField] private GameObject keyObject; // 열쇠

	[Header("Girl")]
	[SerializeField] private Transform girlBox; // 제설함 상자

	private int _placedDollsCount = 0;

	public void PlaceWoodenDoll(string dollId, Transform slot)
	{
		var targetSlot = dollSlots.Find(s => s.slotPosition == slot);
		if (targetSlot == null) return;

		if (targetSlot.requiredDollId == dollId && !targetSlot.isPlaced)
		{
			targetSlot.isPlaced = true;
			_placedDollsCount++;

			Debug.Log($"[BasementPuzzle] 목각인형 배치: {_placedDollsCount}/4");

			CheckSolution();
		}
	}

	protected override bool IsSolutionCorrect()
	{
		return _placedDollsCount >= dollSlots.Count;
	}

	protected override void SolvePuzzle()
	{
		base.SolvePuzzle();

		// 열쇠 획득
		keyObject?.SetActive(true);

		Debug.Log("[BasementPuzzle] 열쇠 획득! 상자를 열 수 있습니다.");
	}
}