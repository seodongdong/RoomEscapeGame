using UnityEngine;

/// <summary>
/// 약봉지 (범인의 정신과 약)
/// </summary>
public class MedicineBottle : MonoBehaviour, IInteractable
{
	[Header("Medicine Info")]
	[SerializeField] private string medicineId = "medicine_bottle";
	[SerializeField] private string medicineName = "약봉지";
	[TextArea(3, 10)]
	[SerializeField] private string description = "정신과에서 처방받은 약이다. 이름이 지워져 있다.";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue = "약봉지가 있다... 무슨 약이지?";

	public string InteractionPrompt => "[F] 약봉지 조사하기";

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(medicineId);
	}

	public void Interact(IPlayer player)
	{
		ClueItem medicine = new ClueItem(medicineId, medicineName, description);
		player.Inventory.AddItem(medicine);
		GameManager.Instance.ClueTracker.RegisterClue(medicineId);

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, dialogue);

		gameObject.SetActive(false);
	}
}