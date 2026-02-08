using UnityEngine;

/// <summary>
/// 1스테이지: 새삥 한복
/// 기획서: 죽은 딸 이름 자수
/// </summary>
public class Stage1_HanbokClue : MonoBehaviour, IInteractable
{
	[Header("Clue Info")]
	[SerializeField] private string clueId = "hanbok_gift";
	[SerializeField] private string clueName = "새삥 한복";
	[SerializeField] private string girlName = "○○○";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue = "한복에 이름이 적혀있다. '{0}'... 누구지?";

	public string InteractionPrompt => "[F] 선물 박스 열기";

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(clueId);
	}

	public void Interact(IPlayer player)
	{
		string description = $"깨끗한 한복이다. 이름 자수에 '{girlName}'이라고 적혀있다.";
		ClueItem clue = new ClueItem(clueId, clueName, description);

		player.Inventory.AddItem(clue);
		GameManager.Instance.ClueTracker.RegisterClue(clueId);

		var uiManager = FindAnyObjectByType<UIManager>();
		string finalDialogue = string.Format(dialogue, girlName);
		uiManager?.ShowDialogue(speaker, finalDialogue);

		gameObject.SetActive(false);
	}
}