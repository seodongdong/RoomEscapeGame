using UnityEngine;

public class VideoTape_Enhanced : MonoBehaviour, IInteractable
{
	[Header("Database")]
	[SerializeField] private VideoTapeDatabase database;
	[SerializeField] private string tapeId;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string pickupDialogue = "낡은 비디오 테이프다.";

	private VideoTapeDatabase.VideoData _videoData;

	public string InteractionPrompt => "[F] 비디오 테이프 획득";

	private void Awake()
	{
		if (database != null)
		{
			_videoData = database.GetVideo(tapeId);
		}
	}

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(tapeId);
	}

	public void Interact(IPlayer player)
	{
		if (_videoData == null) return;

		ClueItem tape = new ClueItem(
			_videoData.tapeId,
			$"비디오 테이프 #{_videoData.stageNumber}",
			_videoData.narration
		);

		player.Inventory.AddItem(tape);

		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(pickupDialogue))
		{
			uiManager?.ShowDialogue(speaker, pickupDialogue);
		}

		gameObject.SetActive(false);
	}
}