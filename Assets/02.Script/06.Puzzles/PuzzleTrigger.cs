using UnityEngine;

/// <summary>
/// 퍼즐 시작 트리거
/// ⭐ OnTriggerEnter/Exit 제거 → Player Raycast 방식으로 통일
/// </summary>
public class PuzzleTrigger : MonoBehaviour, IInteractable
{
	[Header("Puzzle Reference")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("Settings")]
	[SerializeField] private string promptText = "[F] 퍼즐 시작하기";

	private IPuzzle _puzzle;

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;

		if (_puzzle == null && puzzleObject != null)
			Debug.LogError($"[PuzzleTrigger] {puzzleObject.name}은 IPuzzle을 구현하지 않습니다!");
	}

	public string InteractionPrompt => _puzzle != null && _puzzle.IsSolved
		? "[F] (퍼즐 완료)"
		: promptText;

	public bool CanInteract(IPlayer player)
	{
		return _puzzle != null && !_puzzle.IsSolved;
	}

	public void Interact(IPlayer player)
	{
		_puzzle?.StartPuzzle();
	}

	// ❌ OnTriggerEnter/Exit 제거 → Player.cs Raycast가 처리
}