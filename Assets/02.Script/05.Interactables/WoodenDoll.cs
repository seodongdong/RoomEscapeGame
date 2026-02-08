using UnityEngine;

/// <summary>
/// 목각인형 아이템
/// 각 스테이지 퍼즐 완료 시 획득
/// </summary>
public class WoodenDoll : MonoBehaviour, IInteractable
{
	[Header("Doll Info")]
	[SerializeField] private string dollId;
	[SerializeField] private int stageNumber;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue = "나무인형이다. 무언가에 쓸 수 있을 것 같다.";

	public string InteractionPrompt => "[F] 목각인형 획득";

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(dollId);
	}

	public void Interact(IPlayer player)
	{
		ClueItem doll = new ClueItem(dollId, $"목각인형 {stageNumber}", $"{stageNumber}스테이지에서 획득한 목각인형");
		player.Inventory.AddItem(doll);

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, dialogue);

		gameObject.SetActive(false);
	}
}