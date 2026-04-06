using UnityEngine;

public class ExitDoor : MonoBehaviour, IInteractable
{
	[Header("Exit Settings")]
	[SerializeField] private bool requiresGirl = true;
	[SerializeField] private int requiredClueCount = 15;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "문이 굳게 잠겨있다...";
	[TextArea(2, 5)]
	[SerializeField] private string noGirlDialogue = "문이 열리지 않는다... 혼자서는 안 되는 건가?";
	[TextArea(2, 5)]
	[SerializeField] private string notEnoughCluesDialogue = "뭔가... 더 알아야 할 것 같은데...";
	[TextArea(2, 5)]
	[SerializeField] private string successDialogue = "문이 열렸다! 빨리 나가자!";

	[Header("References")]
	[SerializeField] private Girl girlReference;

	private bool _girlRescued;
	private bool _isUnlocked;

	public string InteractionPrompt => "[F] 대문 열기";

	public bool CanInteract(IPlayer player)
	{
		return true; // 항상 상호작용 가능
	}

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();
		var clueTracker = GameManager.Instance.ClueTracker;
		var stageManager = GameManager.Instance.StageManager;

		// 5스테이지 이전에는 잠김
		if (stageManager.CurrentStage < 5)
		{
			uiManager?.ShowDialogue(speaker, lockedDialogue);
			return;
		}

		// 소녀 구출 여부 체크 (추격전 완료 후)
		_girlRescued = girlReference != null && girlReference.gameObject.activeSelf;

		// 단서 개수 체크
		int clueCount = clueTracker.GetClueCount();
		bool hasEnoughClues = clueCount >= requiredClueCount;

		// 조건 1: 소녀 구출 안 됨
		if (requiresGirl && !_girlRescued)
		{
			uiManager?.ShowDialogue(speaker, noGirlDialogue);
			return;
		}

		// 조건 2: 소녀는 있지만 단서 부족 → 노말 엔딩
		if (_girlRescued && !hasEnoughClues)
		{
			uiManager?.ShowDialogue(speaker, notEnoughCluesDialogue);

			// 잠시 후 노말 엔딩
			StartCoroutine(TriggerEndingDelayed(EndingType.Normal, 2f));
		}
		// 조건 3: 모든 조건 충족 → 진엔딩
		else if (_girlRescued && hasEnoughClues)
		{
			uiManager?.ShowDialogue(speaker, successDialogue);

			// 잠시 후 진엔딩
			StartCoroutine(TriggerEndingDelayed(EndingType.True, 2f));
		}
	}

	private System.Collections.IEnumerator TriggerEndingDelayed(EndingType endingType, float delay)
	{
		yield return new WaitForSeconds(delay);
		GameManager.Instance.EndingManager.TriggerEnding(endingType);
	}
}