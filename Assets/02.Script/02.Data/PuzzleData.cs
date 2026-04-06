using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleData", menuName = "Game/Puzzle Data")]
public class PuzzleData : ScriptableObject
{
	[Header("Basic Info")]
	public string puzzleId;
	public string puzzleName;
	public int stageNumber;

	[Header("UI")]
	[TextArea(3, 10)]
	public string hint;
	[TextArea(2, 5)]
	public string successMessage;
	public Sprite puzzleIcon;

	[Header("Reward")]
	public string rewardItemId; // ∆€¡Ò «ÿ∞· Ω√ »πµÊ æ∆¿Ã≈€ (øπ: ø≠ºË)
}