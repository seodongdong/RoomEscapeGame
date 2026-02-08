using UnityEngine;

/// <summary>
/// 1스테이지: 찢어진 일기장
/// 기획서: 누나가 그린 그림 일기
/// </summary>
public class Stage1_DiaryClue : MonoBehaviour, IInteractable
{
	[Header("Clue Info")]
	[SerializeField] private string clueId = "diary_page_1";
	[SerializeField] private string clueName = "찢어진 일기장";
	[TextArea(3, 10)]
	[SerializeField] private string description = "누군가 그린 그림 일기. 스파게티처럼 얽힌 선들과 밝은 색깔의 크레파스 자국이 보인다.";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue = "찢어진 일기장이다... 누가 그린 걸까?";

	public string InteractionPrompt => "[F] 찢어진 일기장 조사하기";

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