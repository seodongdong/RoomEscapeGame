using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform contentParent; // Scroll View Content
    [SerializeField] private TextMeshProUGUI clueCountText;
    [SerializeField] private Button closeButton;
    
    [Header("Prefab")]
    [SerializeField] private GameObject clueCardPrefab;
    
    [Header("Detail Popup")]
    [SerializeField] private GameObject detailPopup;
    [SerializeField] private TextMeshProUGUI detailClueName;
    [SerializeField] private TextMeshProUGUI detailDescription;
    [SerializeField] private Image detailIcon;
    
    private Player _player;
    private List<GameObject> _clueCards = new List<GameObject>();
    private bool _isOpen = false;
    
    private void Start()
    {
        _player = FindAnyObjectByType<Player>();
        
        // 버튼 이벤트 연결
        closeButton.onClick.AddListener(CloseInventory);
        
        // 초기 상태
        inventoryPanel.SetActive(false);
        if (detailPopup != null)
        {
            detailPopup.SetActive(false);
        }
    }
    
    private void Update()
    {
        // I 키로 인벤토리 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_isOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }
        
        // ESC로 닫기
        if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
        {
            CloseInventory();
        }
    }
    
    public void OpenInventory()
    {
        _isOpen = true;
        inventoryPanel.SetActive(true);
        
        // 게임 일시정지
        Time.timeScale = 0;
        
        // 커서 표시
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 단서 목록 갱신
        RefreshClueList();
    }
    
    public void CloseInventory()
    {
        _isOpen = false;
        inventoryPanel.SetActive(false);
        
        // 게임 재개
        Time.timeScale = 1;
        
        // 커서 숨김
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 상세보기 팝업 닫기
        if (detailPopup != null)
        {
            detailPopup.SetActive(false);
        }
    }
    
    private void RefreshClueList()
    {
        // 기존 카드 삭제
        foreach (var card in _clueCards)
        {
            Destroy(card);
        }
        _clueCards.Clear();
        
        if (_player == null || _player.Inventory == null)
        {
            Debug.LogWarning("Player 또는 Inventory가 없습니다.");
            return;
        }
        
        // 인벤토리에서 단서 가져오기
        var inventory = _player.Inventory as PlayerInventory;
        if (inventory == null) return;
        
        // 단서 카드 생성
        var items = inventory.GetAllItems();
        foreach (var item in items)
        {
            if (item.IsClue)
            {
                CreateClueCard(item);
            }
        }
        
        // 단서 개수 업데이트
        int currentCount = items.Count;
        int totalCount = 15; // 전체 단서 개수
        
        if (clueCountText != null)
        {
            clueCountText.text = $"{currentCount} / {totalCount}";
        }
    }
    
    private void CreateClueCard(IItem item)
    {
        if (clueCardPrefab == null || contentParent == null) return;
        
        // 카드 생성
        GameObject card = Instantiate(clueCardPrefab, contentParent);
        _clueCards.Add(card);
        
        // 카드 정보 설정
        var nameText = card.transform.Find("ClueName")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = item.ItemName;
        }
        
        var descText = card.transform.Find("ClueDescription")?.GetComponent<TextMeshProUGUI>();
        if (descText != null)
        {
            // 설명 요약 (처음 30자)
            string shortDesc = item.Description;
            if (shortDesc.Length > 30)
            {
                shortDesc = shortDesc.Substring(0, 30) + "...";
            }
            descText.text = shortDesc;
        }
        
        var iconImage = card.transform.Find("ClueIcon")?.GetComponent<Image>();
        if (iconImage != null && item.Icon != null)
        {
            iconImage.sprite = item.Icon;
        }
        
        // 클릭 이벤트 연결
        var button = card.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ShowClueDetail(item));
        }
    }
    
    private void ShowClueDetail(IItem item)
    {
        if (detailPopup == null) return;
        
        detailPopup.SetActive(true);
        
        if (detailClueName != null)
        {
            detailClueName.text = item.ItemName;
        }
        
        if (detailDescription != null)
        {
            detailDescription.text = item.Description;
        }
        
        if (detailIcon != null && item.Icon != null)
        {
            detailIcon.sprite = item.Icon;
        }
    }
    
    public void CloseDetailPopup()
    {
        if (detailPopup != null)
        {
            detailPopup.SetActive(false);
        }
    }
}