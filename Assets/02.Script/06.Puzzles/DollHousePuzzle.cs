using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DollHousePuzzle : CameraPuzzleBase
{
	[System.Serializable]
	public class DollSlot
	{
		public string itemId;
		public Transform slotTransform;
		public GameObject slotUIElement;
		public GameObject itemPrefab;
		[HideInInspector] public GameObject placedItem;
		[HideInInspector] public bool isPlaced;
	}

	[Header("Doll House Settings")]
	[SerializeField] private List<DollSlot> slots;
	[SerializeField] private Transform itemsContainer;

	[Header("UI Buttons")]
	[SerializeField] private UnityEngine.UI.Button exitButton;

	protected override void Awake()
	{
		base.Awake();

		foreach (var slot in slots)
		{
			slot.isPlaced = false;
			slot.placedItem = null;
		}

		// 나가기 버튼 연결
		if (exitButton != null)
		{
			exitButton.onClick.RemoveAllListeners();
			exitButton.onClick.AddListener(ExitPuzzleButton);
			Debug.Log(">>> 나가기 버튼 리스너 등록 완료");
		}
		else
		{
			Debug.LogError(">>> ExitButton이 연결되지 않았습니다!");
		}
	}

	// 나가기 버튼 클릭 시
	public void ExitPuzzleButton()
	{
		Debug.Log(">>> 나가기 버튼 클릭됨!");
		ExitPuzzle();
	}

	public void TryPlaceItem(string itemId)
	{
		Debug.Log($"=== TryPlaceItem 호출됨: [{itemId}] ===");

		if (_player == null)
		{
			Debug.LogError("Player가 null입니다!");
			return;
		}

		Debug.Log($"Player 확인 완료");

		var inventory = _player.Inventory as PlayerInventory;
		if (inventory != null)
		{
			var allItems = inventory.GetAllItems();
			Debug.Log($">>> 인벤토리 총 개수: {allItems.Count}");
			foreach (var item in allItems)
			{
				Debug.Log($">>> [{item.ItemId}] = {item.ItemName}");
			}
		}

		bool hasItem = _player.Inventory.HasItem(itemId);
		Debug.Log($">>> HasItem({itemId}) = {hasItem}");

		if (!hasItem)
		{
			Debug.Log($"❌ 아이템이 없습니다: [{itemId}]");
			return;
		}

		Debug.Log($"✅ 아이템 있음: [{itemId}]");
		PlaceItem(itemId);
	}

	private void PlaceItem(string itemId)
	{
		var slot = slots.Find(s => s.itemId == itemId);
		if (slot == null || slot.isPlaced) return;

		slot.isPlaced = true;

		if (slot.itemPrefab != null && slot.slotTransform != null)
		{
			slot.placedItem = Instantiate(
				slot.itemPrefab,
				slot.slotTransform.position,
				slot.slotTransform.rotation,
				itemsContainer
			);
		}

		if (slot.slotUIElement != null)
		{
			slot.slotUIElement.SetActive(false);
		}

		Debug.Log($"아이템 배치 완료: {itemId}");

		CheckSolution();
	}

	protected override bool IsSolutionCorrect()
	{
		foreach (var slot in slots)
		{
			if (!slot.isPlaced) return false;
		}
		return true;
	}

	protected override void SolvePuzzle()
	{
		// ⭐ isSolved만 true로 설정
		isSolved = true;

		Debug.Log(">>> 퍼즐 완료!");

		// 성공 대사만 표시
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue("소년", "인형을 모두 찾았다!");

	}

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue("", "인형 부품을 배치하세요");
	}
}