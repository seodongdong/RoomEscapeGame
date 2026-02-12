using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 1스테이지: 인형의 집 퍼즐
/// CameraPuzzleBase 상속 - 카메라 전환 포함
/// </summary>
public class Stage1_DollHousePuzzle : CameraPuzzleBase
{
	[System.Serializable]
	public class DollItem
	{
		public string itemId;
		public Transform targetSlot;
		public GameObject prefab;
		public Vector3 spawnScale = Vector3.one; // ⭐ 아이템마다 크기 조절
	}

	[Header("Items")]
	[SerializeField] private List<DollItem> requiredItems;

	[Header("UI Buttons")]
	[SerializeField] private UnityEngine.UI.Button exitButton;

	[Header("Feedback")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string wrongPositionDialogue = "여기가 아닌 것 같은데...";
	[TextArea(2, 5)]
	[SerializeField] private string correctPositionDialogue = "이 자리가 맞는 것 같아!";
	[TextArea(2, 5)]
	[SerializeField] private string noItemDialogue = "이 아이템이 없다...";
	[TextArea(2, 5)]
	[SerializeField] private string alreadyPlacedDialogue = "이미 배치된 아이템이야.";

	[Header("Tolerance")]
	[SerializeField] private float positionTolerance = 0.5f;

	private Dictionary<string, bool> _placedItems = new Dictionary<string, bool>();

	protected override void Awake()
	{
		base.Awake();

		foreach (var item in requiredItems)
		{
			_placedItems[item.itemId] = false;
		}

		if (exitButton != null)
		{
			exitButton.onClick.RemoveAllListeners();
			exitButton.onClick.AddListener(ExitPuzzleButton);
		}
		else
		{
			Debug.LogWarning("[DollHousePuzzle] Exit Button이 연결되지 않았습니다!");
		}
	}

	public void ExitPuzzleButton()
	{
		ExitPuzzle();
	}

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, "인형 부품을 알맞은 자리에 배치하세요.");
	}

	public void TryPlaceItemToSlot(string itemId)
	{
		if (_player == null || !_player.Inventory.HasItem(itemId))
		{
			ShowFeedback(noItemDialogue);
			return;
		}

		if (_placedItems.ContainsKey(itemId) && _placedItems[itemId])
		{
			ShowFeedback(alreadyPlacedDialogue);
			return;
		}

		var item = requiredItems.Find(i => i.itemId == itemId);
		if (item == null) return;

		PlaceItemToSlot(itemId, item);
	}

	public bool PlaceItem(string itemId, Vector3 position)
	{
		var item = requiredItems.Find(i => i.itemId == itemId);
		if (item == null || item.targetSlot == null) return false;

		float distance = Vector3.Distance(position, item.targetSlot.position);

		if (distance <= positionTolerance)
		{
			PlaceItemToSlot(itemId, item);
			return true;
		}
		else
		{
			ShowFeedback(wrongPositionDialogue);
			return false;
		}
	}

	private void PlaceItemToSlot(string itemId, DollItem item)
	{
		_placedItems[itemId] = true;

		if (item.prefab != null && item.targetSlot != null)
		{
			GameObject spawned = Instantiate(
				item.prefab,
				item.targetSlot.position,
				item.targetSlot.rotation
			);

			// 비활성화된 자식 포함 전체 활성화
			spawned.SetActive(true);
			foreach (Transform child in spawned.GetComponentsInChildren<Transform>(true))
			{
				child.gameObject.SetActive(true);
			}

			// ⭐ Inspector에서 설정한 크기 적용
			spawned.transform.localScale = item.spawnScale;
		}

		var inventoryItem = _player.Inventory.GetItem(itemId);
		if (inventoryItem != null)
			_player.Inventory.RemoveItem(inventoryItem);

		ShowFeedback(correctPositionDialogue);
		CheckSolution();
	}

	private void ShowFeedback(string message)
	{
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, message);
	}

	protected override bool IsSolutionCorrect()
	{
		foreach (var placed in _placedItems.Values)
		{
			if (!placed) return false;
		}
		return true;
	}

	protected override void SolvePuzzle()
	{
		isSolved = true;

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, "인형을 모두 찾았다!");

		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("door_unlock");

		ExitPuzzle();
	}

	private void OnDrawGizmos()
	{
		if (requiredItems == null) return;
		Gizmos.color = Color.green;
		foreach (var item in requiredItems)
		{
			if (item.targetSlot != null)
				Gizmos.DrawWireSphere(item.targetSlot.position, positionTolerance);
		}
	}
}