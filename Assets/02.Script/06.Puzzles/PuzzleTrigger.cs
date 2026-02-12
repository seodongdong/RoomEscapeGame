using UnityEngine;

/// <summary>
/// 퍼즐 시작 트리거
/// ⭐ IPuzzle 인터페이스로 변경 - PuzzleBase, CameraPuzzleBase 모두 드래그 가능
/// </summary>
public class PuzzleTrigger : MonoBehaviour, IInteractable
{
	[Header("Puzzle Reference")]
	[SerializeField] private MonoBehaviour puzzleObject; // ⭐ MonoBehaviour로 변경

	[Header("Settings")]
	[SerializeField] private string promptText = "[F] 퍼즐 시작하기";

	private IPuzzle _puzzle;

	private void Awake()
	{
		// IPuzzle 인터페이스로 캐스팅
		_puzzle = puzzleObject as IPuzzle;

		if (_puzzle == null && puzzleObject != null)
		{
			Debug.LogError($"[PuzzleTrigger] {puzzleObject.name}은 IPuzzle을 구현하지 않습니다!");
		}
	}

	public string InteractionPrompt => promptText;

	public bool CanInteract(IPlayer player)
	{
		return _puzzle != null && !_puzzle.IsSolved;
	}

	public void Interact(IPlayer player)
	{
		_puzzle?.StartPuzzle();
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