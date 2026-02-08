using UnityEngine;

/// <summary>
/// PuzzleData 기반 퍼즐
/// </summary>
public abstract class PuzzleBase_Enhanced : MonoBehaviour, IPuzzle
{
	[Header("Puzzle Data")]
	[SerializeField] protected PuzzleData puzzleData;
	[SerializeField] protected GameObject puzzleUI;

	protected bool _isSolved;

	public string PuzzleId => puzzleData != null ? puzzleData.puzzleId : "";
	public bool IsSolved => _isSolved;

	public event System.Action OnPuzzleSolved;

	public virtual void StartPuzzle()
	{
		if (_isSolved) return;

		GameManager.Instance.StateManager.ChangeState(GameState.Puzzle);
		puzzleUI?.SetActive(true);
		Time.timeScale = 0;

		if (puzzleData != null && !string.IsNullOrEmpty(puzzleData.hint))
		{
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue("", puzzleData.hint);
		}
	}

	public virtual void CheckSolution()
	{
		if (IsSolutionCorrect())
		{
			SolvePuzzle();
		}
	}

	protected abstract bool IsSolutionCorrect();

	protected virtual void SolvePuzzle()
	{
		_isSolved = true;
		puzzleUI?.SetActive(false);
		Time.timeScale = 1;
		GameManager.Instance.StateManager.ChangeState(GameState.Playing);
		OnPuzzleSolved?.Invoke();

		Debug.Log($"[Puzzle] {PuzzleId} 해결!");

		if (puzzleData != null && !string.IsNullOrEmpty(puzzleData.successMessage))
		{
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue("", puzzleData.successMessage);
		}

		if (puzzleData != null && !string.IsNullOrEmpty(puzzleData.rewardItemId))
		{
			GiveReward();
		}
	}

	protected virtual void GiveReward()
	{
		var player = FindAnyObjectByType<Player>();
		if (player != null)
		{
			ClueItem reward = new ClueItem(
				puzzleData.rewardItemId,
				"보상 아이템",
				"퍼즐을 풀어 획득한 아이템"
			);
			player.Inventory.AddItem(reward);
		}
	}

	public virtual void ExitPuzzle()
	{
		puzzleUI?.SetActive(false);
		Time.timeScale = 1;
		GameManager.Instance.StateManager.ChangeState(GameState.Playing);
	}
}