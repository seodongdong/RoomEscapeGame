using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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

	[Header("Puzzle UI")]
	[SerializeField] private GameObject puzzleUI;

	private Player _player;
    private List<GameObject> _clueCards = new List<GameObject>();
    private bool _isOpen = false;

    public bool IsOpen => _isOpen;
    
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
		if (Input.GetKeyDown(KeyCode.I))
		{
			if (inventoryPanel != null && inventoryPanel.activeSelf)
			{
				Debug.Log(">>> I키: 인벤토리 닫기"); // ⭐ 추가
				CloseInventory();
			}
			else
			{
				Debug.Log(">>> I키: 인벤토리 열기"); // ⭐ 추가
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
		Debug.Log(">>> OpenInventory 호출됨");

		_isOpen = true;
        inventoryPanel.SetActive(true);

		if (inventoryPanel != null)
		{
			inventoryPanel.SetActive(true);
			RefreshClueList();

			// ⭐ 퍼즐 UI 비활성화 (한 번만!)
			DisablePuzzleUI(true);
		}

		// 게임 일시정지
		if (GameManager.Instance != null)
		{
			var state = GameManager.Instance.StateManager.CurrentState;
			if (state != GameState.Puzzle)
			{
				Time.timeScale = 0;
			}
		}

		// 커서 표시
		Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


	}
    
    public void CloseInventory()
    {
		Debug.Log(">>> CloseInventory 호출됨!"); // ⭐ 추가

		_isOpen = false;
        inventoryPanel.SetActive(false);

		if (inventoryPanel != null)
		{
			inventoryPanel.SetActive(false);

			// 퍼즐 UI 다시 활성화
			DisablePuzzleUI(false);
		}

		// 게임 재개
		if (GameManager.Instance != null)
		{
			var state = GameManager.Instance.StateManager.CurrentState;
			if (state != GameState.Puzzle)
			{
				Time.timeScale = 1;
			}
		}

		// 커서 숨김
		if (GameManager.Instance != null)
		{
			var state = GameManager.Instance.StateManager.CurrentState;
			if (state == GameState.Puzzle)
			{
				// 퍼즐 중에는 커서 표시 유지
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				// 일반 게임 중에는 커서 숨김
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		// 상세보기 팝업 닫기
		if (detailPopup != null)
        {
            detailPopup.SetActive(false);
        }
    }

	private void DisablePuzzleUI(bool disable)
	{
		// ⭐ GameState가 Puzzle일 때만 작동
		if (GameManager.Instance != null)
		{
			var state = GameManager.Instance.StateManager.CurrentState;
			if (state != GameState.Puzzle)
			{
				Debug.Log(">>> 현재 Puzzle 상태 아님, 스킵");
				return; // Puzzle 상태 아니면 무시
			}
		}

		if (puzzleUI == null)
		{
			Debug.LogWarning(">>> puzzleUI가 null입니다!");
			return;
		}

		Debug.Log($">>> DisablePuzzleUI 실행: {disable}");

		// CanvasGroup으로 차단
		var canvasGroup = puzzleUI.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = puzzleUI.AddComponent<CanvasGroup>();
		}

		canvasGroup.interactable = !disable;
		canvasGroup.blocksRaycasts = !disable;
		canvasGroup.alpha = disable ? 0.5f : 1f;

		// 모든 버튼 비활성화
		var buttons = puzzleUI.GetComponentsInChildren<Button>(true);
		foreach (var button in buttons)
		{
			button.interactable = !disable;
		}

		Debug.Log($">>> 퍼즐 UI 차단 {disable}: 버튼 {buttons.Length}개, interactable={!disable}");
	}

	
	private void RefreshClueList()
{
    Debug.Log("=== RefreshClueList 시작 ===");
    
    // 기존 카드 삭제
    foreach (var card in _clueCards)
    {
        Destroy(card);
    }
    _clueCards.Clear();
    
    if (_player == null || _player.Inventory == null) return;
    
    var inventory = _player.Inventory as PlayerInventory;
    if (inventory == null) return;
    
    // 단서 카드 생성
    var items = inventory.GetAllItems();
    Debug.Log($"✅ 인벤토리에서 가져온 아이템 개수: {items.Count}");
    
    foreach (var item in items)
    {
        if (item.IsClue)
        {
            CreateClueCard(item);
        }
    }
    
    // ⭐ 레이아웃 강제 갱신
    StartCoroutine(ForceLayoutRebuild());
    
    // 단서 개수 업데이트
    int currentCount = items.Count;
    int totalCount = 15;
    
    if (clueCountText != null)
    {
        clueCountText.text = $"{currentCount} / {totalCount}";
    }
    
    Debug.Log($"=== RefreshClueList 완료: {_clueCards.Count}개 카드 생성 ===");
}

// ⭐ 레이아웃 강제 갱신 코루틴
private IEnumerator ForceLayoutRebuild()
{
    // 1프레임 대기
    yield return null;
    
    if (contentParent != null)
    {
        RectTransform contentRect = contentParent as RectTransform;
        
        // Grid Layout Group 비활성화 후 재활성화
        var gridLayout = contentParent.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.enabled = false;
            yield return null;
            gridLayout.enabled = true;
        }
        
        // Content Size Fitter 비활성화 후 재활성화
        var sizeFitter = contentParent.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            sizeFitter.enabled = false;
            yield return null;
            sizeFitter.enabled = true;
        }
        
        // Canvas 강제 업데이트
        Canvas.ForceUpdateCanvases();
        
        // 레이아웃 강제 재구성
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        
        Debug.Log($"✅ 레이아웃 갱신 완료");
        Debug.Log($"   Content 크기: {contentRect.rect.width} x {contentRect.rect.height}");
        Debug.Log($"   자식 개수: {contentParent.childCount}");
        
        // 각 카드 위치 확인
        for (int i = 0; i < contentParent.childCount; i++)
        {
            var child = contentParent.GetChild(i);
            Debug.Log($"   카드 {i}: {child.name}, 위치: {child.localPosition}");
        }
    }
}

private void CreateClueCard(IItem item)
{
    Debug.Log($">>> CreateClueCard 시작: {item.ItemName}");
    
    if (clueCardPrefab == null)
    {
        Debug.LogError("❌ clueCardPrefab이 null입니다!");
        return;
    }
    
    if (contentParent == null)
    {
        Debug.LogError("❌ contentParent가 null입니다!");
        return;
    }
    
    // 카드 생성
    GameObject card = Instantiate(clueCardPrefab, contentParent);
    _clueCards.Add(card);
    
    Debug.Log($"  카드 생성됨: {card.name}");
    Debug.Log($"  부모: {card.transform.parent.name}");
    Debug.Log($"  위치: {card.transform.localPosition}");
    
    // 카드 정보 설정
    var nameText = card.transform.Find("ClueName")?.GetComponent<TextMeshProUGUI>();
    if (nameText != null)
    {
        nameText.text = item.ItemName;
        Debug.Log($"  이름 설정: {item.ItemName}");
    }
    else
    {
        Debug.LogWarning("❌ ClueName을 찾을 수 없습니다!");
    }
    
    var descText = card.transform.Find("ClueDescription")?.GetComponent<TextMeshProUGUI>();
    if (descText != null)
    {
        string shortDesc = item.Description;
        if (shortDesc.Length > 30)
        {
            shortDesc = shortDesc.Substring(0, 30) + "...";
        }
        descText.text = shortDesc;
        Debug.Log($"  설명 설정: {shortDesc}");
    }
    
    var iconImage = card.transform.Find("ClueIcon")?.GetComponent<Image>();
    if (iconImage != null && item.Icon != null)
    {
        iconImage.sprite = item.Icon;
        Debug.Log($"  아이콘 설정");
    }
    
    // 클릭 이벤트 연결
    var button = card.GetComponent<Button>();
    if (button != null)
    {
        button.onClick.AddListener(() => ShowClueDetail(item));
    }
    
    Debug.Log($">>> CreateClueCard 완료: {item.ItemName}");
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