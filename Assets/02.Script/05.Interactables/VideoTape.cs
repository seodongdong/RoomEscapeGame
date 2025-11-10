using UnityEngine;

public class VideoTape : MonoBehaviour, IInteractable
{
	// 테이프 정보
	[Header("Tape Info")]
	[SerializeField] private string tapeId;
	[SerializeField] private int stageNumber;
	[TextArea(3, 10)]
	[SerializeField] private string narration;

	// 상호작용 대사
	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string pickupDialogue = "낡은 비디오 테이프다.";

	// IInteractable 구현
	public string InteractionPrompt => "[F] 비디오 테이프 획득";

	// 상호작용 가능 여부
	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(tapeId);
	}

	// 상호작용 처리
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
