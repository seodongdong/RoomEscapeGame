using UnityEngine;

/// <summary>
/// 퍼즐 시작 트리거
/// </summary>
public class PuzzleTrigger : MonoBehaviour, IInteractable
{
	[Header("Puzzle Reference")]
	[SerializeField] private PuzzleBase puzzle;

	[Header("Settings")]
	[SerializeField] private string promptText = "[F] 퍼즐 시작하기";

	public string InteractionPrompt => promptText;

	public bool CanInteract(IPlayer player)
	{
		return puzzle != null && !puzzle.IsSolved;
	}

	public void Interact(IPlayer player)
	{
		if (puzzle != null)
		{
			puzzle.StartPuzzle();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
		{
			player.SetCurrentInteractable(this);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
		{
			player.SetCurrentInteractable(null);
		}
	}
}