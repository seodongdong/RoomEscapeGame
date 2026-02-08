using UnityEngine;

/// <summary>
/// 3스테이지: 헨젤과 그레텔 일기장
/// </summary>
public class Stage3_Clue : MonoBehaviour, IInteractable
{
	[Header("Clue Info")]
	[SerializeField] private string clueId;
	[SerializeField] private string clueName;
	[TextArea(3, 10)]
	[SerializeField] private string description;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue;

	public string InteractionPrompt => $"[F] {clueName} 조사하기";

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(clueId);
	}

	public void Interact(IPlayer player)
	{
		ClueItem clue = new ClueItem(clueId, clueName, description);
		player.Inventory.AddItem(clue);
		GameManager.Instance.ClueTracker.RegisterClue(clueId);

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, dialogue);

		gameObject.SetActive(false);
	}
}