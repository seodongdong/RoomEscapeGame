using UnityEngine;

/// <summary>
/// 사용 가능한 아이템 단서 — InteractableBase + ClueRegistrar 전환판
/// (예: 열쇠, 도구 등. 기획서 분류: "수집 및 사용 가능 단서")
///
/// [리팩토링 변경점]
/// DiaryClue와 동일한 방식으로 전환했습니다.
/// ISaveRestorable은 그대로 유지합니다 — 저장/불러오기 복원 책임은
/// 그대로 보존했습니다.
/// </summary>
public class UsableItemClue : InteractableBase, ISaveRestorable
{
	[Header("Item Info")]
	[SerializeField] private string itemId = "key_bedroom";
	[SerializeField] private string itemName = "침실 열쇠";

	[Header("Inventory Data")]
	[SerializeField] private string itemDate = "2023.07.16";
	[TextArea(3, 5)]
	[SerializeField] private string description = "낡은 침실 열쇠. 녹슬어 있지만 아직 사용할 수 있을 것 같다.";
	[SerializeField] private GameObject itemPrefab;

	[Header("First Interaction Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string firstDialogue = "열쇠를 발견했다. 어디에 쓰는 열쇠일까?";

	private bool _hasCollected = false;

	// ISaveRestorable
	public string RestoreItemId => itemId;
	public void ApplyAlreadyCollected()
	{
		_hasCollected = true;
		gameObject.SetActive(false);
	}

	public override string InteractionPrompt => _hasCollected ? "" : $"[F] {itemName} 획득";

	public override bool CanInteract(IPlayer player) => !_hasCollected;

	protected override void OnInteract(IPlayer player)
	{
		if (_hasCollected) return;
		_hasCollected = true;

		if (!string.IsNullOrEmpty(firstDialogue))
			GameServices.UI?.ShowDialogue(speaker, firstDialogue);

		// 기존 PlayerInventory + InventoryUI + ClueTracker 3중 등록 → 1줄
		ClueRegistrar.RegisterUsableItem(player, itemId, itemName, itemDate, description, itemPrefab);

		gameObject.SetActive(false);

		Debug.Log($"[UsableItemClue] {itemName} 획득 완료");
	}
}
