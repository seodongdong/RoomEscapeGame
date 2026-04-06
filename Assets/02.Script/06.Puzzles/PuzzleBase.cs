using UnityEngine;

// 퍼즐의 기본 동작을 정의하는 추상 클래스
public abstract class PuzzleBase : MonoBehaviour, IPuzzle
{
	// 퍼즐 고유 ID
	[SerializeField] protected string puzzleId;
	[SerializeField] protected GameObject puzzleUI;

	// 퍼즐 해결 상태
	protected bool _isSolved;

	// 퍼즐 ID 속성
	public string PuzzleId => puzzleId;
	public bool IsSolved => _isSolved;

	// 퍼즐 해결 이벤트
	public event System.Action OnPuzzleSolved;

	// 퍼즐 시작 메서드
	public virtual void StartPuzzle()
	{
		if (_isSolved) return;

		GameManager.Instance.StateManager.ChangeState(GameState.Puzzle);
		puzzleUI?.SetActive(true);
		Time.timeScale = 0;

		// 커서 표시
		Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

		Debug.Log($"퍼즐 시작: {puzzleId}");
	}

	// 퍼즐 해결 확인 메서드
	public virtual void CheckSolution()
	{
		if (IsSolutionCorrect())
		{
			SolvePuzzle();
		}
	}

	// 퍼즐 해결 여부를 확인하는 추상 메서드
	protected abstract bool IsSolutionCorrect();

	// 퍼즐 해결 처리 메서드
	protected virtual void SolvePuzzle()
	{
		_isSolved = true;
		puzzleUI?.SetActive(false);
		Time.timeScale = 1;

		// 커서 숨기기
		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

		GameManager.Instance.StateManager.ChangeState(GameState.Playing);
		OnPuzzleSolved?.Invoke();
		Debug.Log($"퍼즐 해결: {puzzleId}");
	}

	// 퍼즐 종료 메서드
	public virtual void ExitPuzzle()
	{
		puzzleUI?.SetActive(false);
		Time.timeScale = 1;

		// 커서 숨기기
		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

		GameManager.Instance.StateManager.ChangeState(GameState.Playing);
	}
}
