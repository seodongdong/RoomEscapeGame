using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

/// <summary>
/// 2스테이지: 제단 사탕 퍼즐
/// 기획서: "맵을 참고해서 사탕을 방석 위 인형과 같은 위치로"
/// </summary>
public class Stage2_AltarCandyPuzzle : PuzzleBase
{
	[System.Serializable]
	public class CandySlot
	{
		public int position;            // 0~15 (방석 위치)
		public Color requiredColor;     // 필요한 사탕 색
		public Transform slotTransform;
		public GameObject candyVisual;
	}

	[Header("Candy Slots")]
	[SerializeField] private List<CandySlot> candySlots; // 5개

	[Header("Reset")]
	[SerializeField] private Transform resetIncense; // 리셋 향로

	[Header("Creature")]
	[SerializeField] private Stage2_ShadowCreature shadowCreature; // 작은 방 진입 담당

	private Dictionary<int, Color> _currentPlacements = new Dictionary<int, Color>();

	public void PlaceCandy(int position, Color color)
	{
		_currentPlacements[position] = color;

		var slot = candySlots.Find(s => s.position == position);
		if (slot != null && slot.candyVisual != null)
		{
			slot.candyVisual.GetComponent<Renderer>().material.color = color;
			slot.candyVisual.SetActive(true);
		}

		CheckSolution();
	}

	public void ResetPuzzle()
	{
		_currentPlacements.Clear();

		foreach (var slot in candySlots)
		{
			if (slot.candyVisual != null)
			{
				slot.candyVisual.SetActive(false);
			}
		}

		Debug.Log("[AltarPuzzle] 퍼즐 리셋");
	}

	protected override bool IsSolutionCorrect()
	{
		if (_currentPlacements.Count != candySlots.Count) return false;

		foreach (var slot in candySlots)
		{
			if (!_currentPlacements.ContainsKey(slot.position)) return false;
			if (_currentPlacements[slot.position] != slot.requiredColor) return false;
		}
		return true;
	}

	protected override void SolvePuzzle()
	{
		base.SolvePuzzle();

		// 작은 방 진입 가능
		Debug.Log("[AltarPuzzle] 작은 방 진입 가능!");

		if(shadowCreature != null)
		{
			shadowCreature.MoveToFinalPosition();
		}
	}

	public override void ExitPuzzle()
	{
		base.ExitPuzzle();
	}
}