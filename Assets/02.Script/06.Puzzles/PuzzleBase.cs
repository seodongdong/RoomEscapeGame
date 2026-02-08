using UnityEngine;

/// <summary>
/// 퍼즐 베이스 클래스
/// </summary>
public abstract class PuzzleBase : MonoBehaviour, IPuzzle
{
	[SerializeField] protected string puzzleId;
	[SerializeField] protected GameObject puzzleUI;

	protected bool _isSolved;

	public string PuzzleId => puzzleId;
	public bool IsSolved => _isSolved;

	public event System.Action OnPuzzleSolved;

	public virtual void StartPuzzle()
	{
		if (_isSolved) return;

		GameManager.Instance.StateManager.ChangeState(GameState.Puzzle);
		puzzleUI?.SetActive(true);
		Time.timeScale = 0;

		Debug.Log($"[Puzzle] {puzzleId} 시작");
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

		Debug.Log($"[Puzzle] {puzzleId} 해결!");
	}

	public virtual void ExitPuzzle()
	{
		puzzleUI?.SetActive(false);
		Time.timeScale = 1;
		GameManager.Instance.StateManager.ChangeState(GameState.Playing);
	}
}