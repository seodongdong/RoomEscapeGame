using UnityEngine;

/// <summary>
/// 책가방 단서
/// </summary>
public class Backpack : MonoBehaviour, IInteractable
{
	[Header("Backpack Info")]
	[SerializeField] private string backpackId = "backpack";
	[SerializeField] private string ownerName;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue = "누군가의 책가방이다...";

	public string InteractionPrompt => "[F] 책가방 조사하기";

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(backpackId);
	}

	public void Interact(IPlayer player)
	{
		string description = string.IsNullOrEmpty(ownerName)
			? "낡은 책가방이다."
			: $"{ownerName}의 책가방이다.";

		ClueItem backpack = new ClueItem(backpackId, "책가방", description);
		player.Inventory.AddItem(backpack);
		GameManager.Instance.ClueTracker.RegisterClue(backpackId);

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, dialogue);

		gameObject.SetActive(false);
	}
}