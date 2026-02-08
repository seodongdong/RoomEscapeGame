using UnityEngine;

/// <summary>
/// 비디오테이프 아이템
/// </summary>
public class VideoTape : MonoBehaviour, IInteractable
{
	[Header("Tape Info")]
	[SerializeField] private string tapeId;
	[SerializeField] private int stageNumber;
	[TextArea(5, 15)]
	[SerializeField] private string narration;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string pickupDialogue = "낡은 비디오 테이프다.";

	public string InteractionPrompt => "[F] 비디오 테이프 획득";

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(tapeId);
	}

	public void Interact(IPlayer player)
	{
		ClueItem tape = new ClueItem(tapeId, $"비디오 테이프 #{stageNumber}", narration);
		player.Inventory.AddItem(tape);

		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(pickupDialogue))
		{
			uiManager?.ShowDialogue(speaker, pickupDialogue);
		}

		gameObject.SetActive(false);
	}
}