using UnityEngine;

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

	private bool _isUnlocked;

	public string InteractionPrompt => "[F] 문 열기";

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();

		// 퍼즐 해결 필요한 경우
		if (requiresPuzzleSolved && !string.IsNullOrEmpty(requiredPuzzleId))
		{
			var puzzle = FindObjectsOfType<PuzzleBase>();
			bool puzzleSolved = false;

			foreach (var p in puzzle)
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

		// 전환 가능
		uiManager?.ShowDialogue(speaker, openDialogue);

		// 다음 스테이지로 이동
		StartCoroutine(TransitionToNextStage());
	}

	private System.Collections.IEnumerator TransitionToNextStage()
	{
		yield return new WaitForSeconds(2f);

		GameManager.Instance.StageManager.LoadStage(nextStageNumber);
	}
}