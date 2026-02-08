using UnityEngine;

/// <summary>
/// 캠코더 (진엔딩 조건)
/// 기획서: "추격전 하면서 창고에 있는 캠코더 주워서 나가야함"
/// </summary>
public class Camcorder : MonoBehaviour, IInteractable
{
	[Header("Settings")]
	[SerializeField] private string itemId = "camcorder";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue = "캠코더다...! 이게 왜 여기 있지?";

	public string InteractionPrompt => "[F] 캠코더 획득";

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(itemId);
	}

	public void Interact(IPlayer player)
	{
		ClueItem camcorder = new ClueItem(itemId, "캠코더", "진실을 담은 캠코더");
		player.Inventory.AddItem(camcorder);

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, dialogue);

		Debug.Log("[Camcorder] 진엔딩 조건 충족!");

		gameObject.SetActive(false);
	}
}