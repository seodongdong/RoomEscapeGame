using UnityEngine;

public class PuzzleTrigger : MonoBehaviour, IInteractable
{
	[Header("Puzzle Reference")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("Prompts")]
	[SerializeField] private string promptText = "[F] 살펴보기";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string solvedDialogue = "";

	private IPuzzle _puzzle;

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;
		if (_puzzle == null && puzzleObject != null)
			Debug.LogError($"[PuzzleTrigger] {puzzleObject.name}은 IPuzzle을 구현하지 않습니다!");
	}

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt
	{
		get
		{
			// ★ 퍼즐 완료 후 프롬프트 완전히 숨김
			if (_puzzle == null || _puzzle.IsSolved) return "";
			return promptText;
		}
	}

	public bool CanInteract(IPlayer player)
	{
		// ★ 퍼즐 완료 후 상호작용 완전히 차단
		if (_puzzle == null || _puzzle.IsSolved) return false;
		return true;
	}

	public void Interact(IPlayer player)
	{
		if (Stage1TVGate.CheckPriorityBlocked(player)) return;
		if (_puzzle == null || _puzzle.IsSolved) return;
		_puzzle.StartPuzzle();
	}
}