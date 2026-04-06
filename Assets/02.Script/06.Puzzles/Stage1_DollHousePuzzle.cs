using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 1스테이지: 인형의 집 퍼즐
/// CameraPuzzleBase 상속 - 카메라 전환 포함
///
/// [버그 수정] DollHouseSlotButton 제거 - 슬롯 버튼 onClick이 이미 Inspector에서
///            TryPlaceItemToSlot에 연결되어 있어서 DollHouseSlotButton이 중복 호출 유발
/// </summary>
public class Stage1_DollHousePuzzle : CameraPuzzleBase
{
    [System.Serializable]
    public class DollItem
    {
        public string itemId;
        public Transform targetSlot;
        public GameObject prefab;
        public Vector3 spawnScale = Vector3.one;
    }

    [Header("Items")]
    [SerializeField] private List<DollItem> requiredItems;

    [Header("UI Buttons")]
    [SerializeField] private UnityEngine.UI.Button exitButton;

    [Header("Feedback")]
    [SerializeField] private string speaker = "소년";
    [TextArea(2, 5)]
    [SerializeField] private string correctPositionDialogue = "이 자리가 맞는 것 같아!";
    [TextArea(2, 5)]
    [SerializeField] private string noItemDialogue = "이 아이템이 없다...";
    [TextArea(2, 5)]
    [SerializeField] private string alreadyPlacedDialogue = "이미 배치된 아이템이야.";

    [Header("Tolerance")]
    [SerializeField] private float positionTolerance = 0.5f;

    [Header("Creature")]
    [SerializeField] private GameObject creature;

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
    }

    public void ExitPuzzleButton()
    {
        ExitPuzzle();
    }

    protected override void OnPuzzleStarted()
    {
        base.OnPuzzleStarted();
        ShowFeedback("인형 부품을 알맞은 자리에 배치하세요.");
    }

    /// <summary>
    /// 슬롯 버튼 onClick에서 직접 호출 (Inspector에서 연결)
    /// DollHouseSlotButton 스크립트 없이 이것만 사용
    /// </summary>
    public void TryPlaceItemToSlot(string itemId)
    {
        if (_player == null)
        {
            Debug.LogError("[DollHousePuzzle] _player가 null입니다!");
            return;
        }

        // 이미 배치된 슬롯인지 먼저 체크
        if (_placedItems.ContainsKey(itemId) && _placedItems[itemId])
        {
            ShowFeedback(alreadyPlacedDialogue);
            return;
        }

        // 아이템 보유 여부 체크
        if (!_player.Inventory.HasItem(itemId))
        {
            ShowFeedback(noItemDialogue);
            return;
        }

        var item = requiredItems.Find(i => i.itemId == itemId);
        if (item == null)
        {
            Debug.LogError($"[DollHousePuzzle] requiredItems에 itemId={itemId}가 없습니다!");
            return;
        }

        PlaceItemToSlot(itemId, item);
    }

	private void PlaceItemToSlot(string itemId, DollItem item)
	{
		_placedItems[itemId] = true;

		// 3D 프리팹 생성
		if (item.prefab != null && item.targetSlot != null)
		{
			GameObject spawned = Instantiate(
				item.prefab,
				item.targetSlot.position,
				item.targetSlot.rotation
			);
			spawned.SetActive(true);
			foreach (Transform child in spawned.GetComponentsInChildren<Transform>(true))
				child.gameObject.SetActive(true);
			spawned.transform.localScale = item.spawnScale;
		}

		// ✅ PlayerInventory에서 제거 (한 번만)
		var inventoryItem = _player.Inventory.GetItem(itemId);
		if (inventoryItem != null)
			_player.Inventory.RemoveItem(inventoryItem);

		// ✅ InventoryUI에서도 제거 (화면 갱신)
		var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
		inventoryUI?.RemoveItem(itemId);

		ShowFeedback(correctPositionDialogue);
		Debug.Log($"[DollHousePuzzle] {itemId} 배치 완료!");

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
        ShowFeedback("인형을 모두 찾았다!");
        var audioManager = FindAnyObjectByType<AudioManager>();
        audioManager?.PlaySFX("door_unlock");
        ExitPuzzle();

        base.SolvePuzzle();
        if (creature != null)
        {
            creature.SetActive(false);
		}
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