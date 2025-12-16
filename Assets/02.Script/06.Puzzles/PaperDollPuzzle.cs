using UnityEngine;
using System.Collections.Generic;

public class PaperDollPuzzle : PuzzleBase
{
	public enum DollPart
	{
		Head,
		Body,
		Arms,
		Legs,
		Dress
	}

	[System.Serializable]
	public class PartOption
	{
		public DollPart partType;
		public string partId;
		public bool isCorrect;
		public Sprite sprite;
	}

	[Header("Puzzle Settings")]
	[SerializeField] private List<PartOption> availableParts;

	[Header("Fail Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string failDialogue = "...! 인형이 찢어졌다.";

	private Dictionary<DollPart, string> _selectedParts = new Dictionary<DollPart, string>();

	public void SelectPart(DollPart partType, string partId)
	{
		_selectedParts[partType] = partId;
		Debug.Log($"부품 선택: {partType} - {partId}");

		var part = availableParts.Find(p => p.partId == partId);
		if (part != null && !part.isCorrect)
		{
			TriggerFailEffect();
			_selectedParts.Remove(partType);
		}
		else
		{
			CheckSolution();
		}
	}

	private void TriggerFailEffect()
	{
		Debug.Log("잘못된 부품! 인형이 찢어집니다.");

		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(failDialogue))
		{
			uiManager?.ShowDialogue(speaker, failDialogue);
		}
	}

	protected override bool IsSolutionCorrect()
	{
		if (_selectedParts.Count != System.Enum.GetValues(typeof(DollPart)).Length)
			return false;

		foreach (var selected in _selectedParts)
		{
			var part = availableParts.Find(p =>
				p.partType == selected.Key && p.partId == selected.Value);

			if (part == null || !part.isCorrect)
				return false;
		}
		return true;
	}
}