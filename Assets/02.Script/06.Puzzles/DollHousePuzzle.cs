using UnityEngine;
using System.Collections.Generic;

public class DollHousePuzzle : CameraPuzzleBase
{
    [System.Serializable]
    public class DollSlot
    {
        public string itemId;              // "doll_head"
        public Transform slotTransform;    // 3D 슬롯 위치
        public GameObject slotUIElement;   // UI 슬롯 (투명 버튼)
        public GameObject itemPrefab;      // 배치될 3D 아이템
        [HideInInspector] public GameObject placedItem; // 실제 생성된 아이템
        [HideInInspector] public bool isPlaced;
    }

    [Header("Doll House Settings")]
    [SerializeField] private List<DollSlot> slots;
    [SerializeField] private Transform itemsContainer; // 배치된 아이템들의 부모
    
    [Header("UI Buttons")]
    [SerializeField] private UnityEngine.UI.Button exitButton;

    protected override void Awake()
    {
        base.Awake();
        
        // 초기화
        foreach (var slot in slots)
        {
            slot.isPlaced = false;
            slot.placedItem = null;
        }
        
        // 나가기 버튼 연결
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitPuzzle);
        }
    }

    // UI 슬롯 버튼에서 호출
    public void TryPlaceItem(string itemId)
    {
        // 플레이어가 아이템을 가지고 있는지 확인
        if (_player == null || !_player.Inventory.HasItem(itemId))
        {
            Debug.Log($"아이템이 없습니다: {itemId}");
            return;
        }

        PlaceItem(itemId);
    }

    private void PlaceItem(string itemId)
    {
        var slot = slots.Find(s => s.itemId == itemId);
        if (slot == null || slot.isPlaced) return;

        slot.isPlaced = true;
        
        // 3D 아이템을 슬롯 위치에 생성
        if (slot.itemPrefab != null && slot.slotTransform != null)
        {
            slot.placedItem = Instantiate(
                slot.itemPrefab, 
                slot.slotTransform.position, 
                slot.slotTransform.rotation, 
                itemsContainer
            );
        }
        
        // UI 슬롯 비활성화 (이미 배치됨)
        if (slot.slotUIElement != null)
        {
            slot.slotUIElement.SetActive(false);
        }
        
        Debug.Log($"아이템 배치 완료: {itemId}");
        
        // 퍼즐 완성 체크
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
        base.SolvePuzzle();
        
        // 성공 대사
        var uiManager = FindAnyObjectByType<UIManager>();
        uiManager?.ShowDialogue("소년", "인형을 모두 찾았다!");
    }

    protected override void OnPuzzleStarted()
    {
        base.OnPuzzleStarted();
        
        // 안내 대사 (선택)
        var uiManager = FindAnyObjectByType<UIManager>();
        uiManager?.ShowDialogue("", "인형 부품을 배치하세요");
    }
}