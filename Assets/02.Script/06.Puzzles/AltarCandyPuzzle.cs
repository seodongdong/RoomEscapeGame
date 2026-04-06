using UnityEngine;
using System.Collections.Generic;

public class AltarCandyPuzzle : PuzzleBase
{
	[System.Serializable]
	public class CandySlot
	{
		public int position;
		public Color requiredColor;
		public Transform slotTransform;
		public GameObject candyVisual;
	}

	[SerializeField] private List<CandySlot> candySlots;
	[SerializeField] private Transform resetIncense;

	private Dictionary<int, Color> _currentPlacements = new Dictionary<int, Color>();

	public void PlaceCandy(int position, Color color)
	{
		_currentPlacements[position] = color;

		var slot = candySlots.Find(s => s.position == position);
		if (slot != null && slot.candyVisual != null)
		{
			slot.candyVisual.GetComponent<Renderer>().material.color = color;
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
	}
}