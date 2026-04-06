using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 일기장 UI 시스템
/// - 좌/우 버튼으로 페이지 넘기기
/// - 여러 페이지 지원
/// - 닫기: 인벤토리에서 열었을 때는 인벤토리로 복귀, 아니면 게임으로 복귀
/// 
/// [버그 수정] 닫기 버튼이 인벤토리까지 같이 닫히던 문제 수정
/// - 원인: CloseDiary()가 항상 Playing 상태로 복귀 + 커서 잠금
///         그런데 ReadDocument()에서 CloseInventory() 후 OpenDiary() 순서라
///         인벤토리는 이미 닫혔는데 플레이어까지 활성화되어 버림
/// - 수정: returnToInventory 플래그로 닫기 시 분기 처리
/// </summary>
public class DiaryUI : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private GameObject diaryPanel;
	[SerializeField] private TextMeshProUGUI pageText;
	[SerializeField] private TextMeshProUGUI pageNumberText;
	[SerializeField] private Button leftButton;
	[SerializeField] private Button rightButton;
	[SerializeField] private Button closeButton;

	private List<string> _pages = new List<string>();
	private int _currentPage = 0;
	private Player _player;
	private InventoryUI_Complete _inventoryUI;

	// 닫을 때 인벤토리로 돌아갈지 여부
	private bool _returnToInventory = false;

	private void Awake()
	{
		if (diaryPanel != null)
			diaryPanel.SetActive(false);

		if (leftButton != null)
			leftButton.onClick.AddListener(PreviousPage);

		if (rightButton != null)
			rightButton.onClick.AddListener(NextPage);

		if (closeButton != null)
			closeButton.onClick.AddListener(CloseDiary);
	}

	private void Start()
	{
		_player = FindAnyObjectByType<Player>();
		_inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
	}

	/// <summary>
	/// 일기장 열기
	/// </summary>
	/// <param name="pages">표시할 페이지 목록</param>
	/// <param name="returnToInventory">닫을 때 인벤토리로 돌아갈지 여부 (인벤토리 읽기 버튼에서 호출 시 true)</param>
	public void OpenDiary(List<string> pages, bool returnToInventory = false)
	{
		if (pages == null || pages.Count == 0)
		{
			Debug.LogWarning("[DiaryUI] 페이지가 없습니다!");
			return;
		}

		_pages = pages;
		_currentPage = 0;
		_returnToInventory = returnToInventory;

		diaryPanel?.SetActive(true);
		UpdatePage();

		// 플레이어 조작 비활성화
		if (_player != null)
			_player.enabled = false;

		// 게임 상태 변경
		GameManager.Instance?.StateManager.ChangeState(GameState.Puzzle);

		// 커서 표시
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		Debug.Log($"[DiaryUI] 일기장 열림 (총 {_pages.Count}페이지, 인벤토리 복귀: {returnToInventory})");
	}

	/// <summary>
	/// 일기장 닫기
	/// - returnToInventory == true  → 인벤토리 다시 열기
	/// - returnToInventory == false → 게임으로 복귀
	/// </summary>
	public void CloseDiary()
	{
		diaryPanel?.SetActive(false);

		if (_returnToInventory && _inventoryUI != null)
		{
			// 인벤토리로 복귀 (인벤토리가 플레이어/커서/상태 처리를 담당)
			_inventoryUI.OpenInventory();
			Debug.Log("[DiaryUI] 일기장 닫힘 → 인벤토리 복귀");
		}
		else
		{
			// 게임으로 직접 복귀
			if (_player != null)
				_player.enabled = true;

			GameManager.Instance?.StateManager.ChangeState(GameState.Playing);

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			Debug.Log("[DiaryUI] 일기장 닫힘 → 게임 복귀");
		}
	}

	/// <summary>이전 페이지</summary>
	public void PreviousPage()
	{
		if (_currentPage > 0)
		{
			_currentPage--;
			UpdatePage();
		}
	}

	/// <summary>다음 페이지</summary>
	public void NextPage()
	{
		if (_currentPage < _pages.Count - 1)
		{
			_currentPage++;
			UpdatePage();
		}
	}

	private void UpdatePage()
	{
		if (pageText != null && _currentPage < _pages.Count)
			pageText.text = _pages[_currentPage];

		if (pageNumberText != null)
			pageNumberText.text = $"{_currentPage + 1} / {_pages.Count}";

		if (leftButton != null)
			leftButton.interactable = _currentPage > 0;

		if (rightButton != null)
			rightButton.interactable = _currentPage < _pages.Count - 1;
	}
}