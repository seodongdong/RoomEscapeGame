using UnityEngine;
using System.Collections;

/// <summary>
/// 스테이지 전환 문
/// 퍼즐 해결 시 다음 스테이지로 이동
/// </summary>
public class StageTransition : MonoBehaviour, IInteractable
{
	[Header("Transition Settings")]
	[SerializeField] private int nextStageNumber;
	[SerializeField] private bool requiresPuzzleSolved;
	[SerializeField] private string requiredPuzzleId;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "문이 잠겨있다...";
	[TextArea(2, 5)]
	[SerializeField] private string openDialogue = "다음 방으로 가보자.";

	[Header("Effects")]
	[SerializeField] private float transitionDelay = 2f;

	public string InteractionPrompt => "[F] 문 열기";

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();

		if (requiresPuzzleSolved && !string.IsNullOrEmpty(requiredPuzzleId))
		{
			var puzzles = FindObjectsOfType<PuzzleBase>();
			bool puzzleSolved = false;

			foreach (var p in puzzles)
			{
				if (p.PuzzleId == requiredPuzzleId && p.IsSolved)
				{
					puzzleSolved = true;
					break;
				}
			}

			if (!puzzleSolved)
			{
				uiManager?.ShowDialogue(speaker, lockedDialogue);
				return;
			}
		}

		uiManager?.ShowDialogue(speaker, openDialogue);
		StartCoroutine(TransitionToNextStage());
	}

	private IEnumerator TransitionToNextStage()
	{
		yield return new WaitForSeconds(transitionDelay);

		GameManager.Instance.StageManager.LoadStage(nextStageNumber);
	}
}