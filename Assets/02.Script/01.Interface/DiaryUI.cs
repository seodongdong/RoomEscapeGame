using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 일기장 UI 시스템
/// - 좌/우 클릭으로 페이지 넘기기
/// - 여러 페이지 지원
/// - ESC로 닫기
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

	[Header("Page Navigation Areas")]
	[SerializeField] private RectTransform leftClickArea;   // 화면 왼쪽 절반
	[SerializeField] private RectTransform rightClickArea;  // 화면 오른쪽 절반

	private List<string> _pages = new List<string>();
	private int _currentPage = 0;
	private Player _player;

	private void Awake()
	{
		if (diaryPanel != null)
			diaryPanel.SetActive(false);

		// 버튼 이벤트 연결
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
	}

	/// <summary>
	/// 일기장 열기
	/// </summary>
	public void OpenDiary(List<string> pages)
	{
		if (pages == null || pages.Count == 0)
		{
			Debug.LogWarning("[DiaryUI] 페이지가 없습니다!");
			return;
		}

		_pages = pages;
		_currentPage = 0;

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

		Debug.Log($"[DiaryUI] 일기장 열림 (총 {_pages.Count}페이지)");
	}

	/// <summary>
	/// 일기장 닫기
	/// </summary>
	public void CloseDiary()
	{
		diaryPanel?.SetActive(false);

		// 플레이어 복귀
		if (_player != null)
			_player.enabled = true;

		// 게임 상태 복귀
		GameManager.Instance?.StateManager.ChangeState(GameState.Playing);

		// 커서 잠금
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		Debug.Log("[DiaryUI] 일기장 닫힘");
	}

	/// <summary>
	/// 이전 페이지
	/// </summary>
	public void PreviousPage()
	{
		if (_currentPage > 0)
		{
			_currentPage--;
			UpdatePage();
			Debug.Log($"[DiaryUI] 이전 페이지: {_currentPage + 1}/{_pages.Count}");
		}
	}

	/// <summary>
	/// 다음 페이지
	/// </summary>
	public void NextPage()
	{
		if (_currentPage < _pages.Count - 1)
		{
			_currentPage++;
			UpdatePage();
			Debug.Log($"[DiaryUI] 다음 페이지: {_currentPage + 1}/{_pages.Count}");
		}
	}

	/// <summary>
	/// 페이지 내용 업데이트
	/// </summary>
	private void UpdatePage()
	{
		if (pageText != null && _currentPage < _pages.Count)
		{
			pageText.text = _pages[_currentPage];
		}

		if (pageNumberText != null)
		{
			pageNumberText.text = $"{_currentPage + 1} / {_pages.Count}";
		}

		// 버튼 활성화/비활성화
		if (leftButton != null)
			leftButton.interactable = _currentPage > 0;

		if (rightButton != null)
			rightButton.interactable = _currentPage < _pages.Count - 1;
	}

	private void Update()
	{
		if (!diaryPanel.activeSelf) return;

		// 아무 입력도 처리하지 않음 - 버튼만 사용
	}
}