using UnityEngine;

/// <summary>
/// 퍼즐 시작 트리거
///
/// [기획서 기준]
/// - 오브젝트에 시점 맞추고 F키 → 퍼즐 시작. 프롬프트: "살펴보기"
/// - 퍼즐 완료 후에도 상호작용 가능 (오브젝트가 사라지지 않음)
/// - 단, 완료된 퍼즐을 다시 풀거나 움직일 수 없음
///
/// [변경]
/// - 완료 후 promptText → solvedPromptText로 교체
/// - CanInteract → 완료 후에도 true (상호작용 가능, Interact에서 분기)
/// - Interact → 완료 상태면 solvedDialogue 출력 후 리턴
/// </summary>
public class PuzzleTrigger : MonoBehaviour, IInteractable
{
	[Header("Puzzle Reference")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("Prompts")]
	[SerializeField] private string promptText = "[F] 살펴보기";
	[SerializeField] private string solvedPromptText = "[F] 살펴보기";  // 완료 후 프롬프트

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string solvedDialogue = "";  // 비워두면 대사 없음

	private IPuzzle _puzzle;

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;

		if (_puzzle == null && puzzleObject != null)
			Debug.LogError(
				$"[PuzzleTrigger] {puzzleObject.name}은 IPuzzle을 구현하지 않습니다!");
	}

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt
	{
		get
		{
			if (_puzzle == null) return promptText;
			return _puzzle.IsSolved ? solvedPromptText : promptText;
		}
	}

	/// <summary>
	/// ★ 기획서: 완료 후에도 상호작용 가능 (오브젝트가 씬에 남아있음)
	/// </summary>
	public bool CanInteract(IPlayer player) => _puzzle != null;

	public void Interact(IPlayer player)
	{
		if (Stage1TVGate.CheckPriorityBlocked(player)) return;
		if (_puzzle == null) return;

		// 퍼즐 완료 상태
		if (_puzzle.IsSolved)
		{
			// 완료 대사가 있으면 출력하고 끝 (다시 풀기 불가)
			if (!string.IsNullOrEmpty(solvedDialogue))
				GameServices.UI?.ShowDialogue(speaker, solvedDialogue);
			return;
		}

		// 퍼즐 시작
		_puzzle.StartPuzzle();
	}
}