using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 문서 전체화면 뷰어
/// - 페이지 넘기기
/// - 재열람 가능
/// </summary>
public class DocumentViewerUI : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private GameObject viewerPanel;
	[SerializeField] private Image documentImage;           // 문서 이미지
	[SerializeField] private TextMeshProUGUI titleText;     // 문서 제목
	[SerializeField] private TextMeshProUGUI pageText;      // 페이지 표시 (1/3)
	[SerializeField] private TextMeshProUGUI contentText;   // 텍스트 내용 (선택)
	[SerializeField] private Button prevButton;             // 이전 페이지
	[SerializeField] private Button nextButton;             // 다음 페이지
	[SerializeField] private Button closeButton;            // 닫기 버튼

	private Sprite[] _currentPages;
	private int _currentPageIndex = 0;
	private string _currentTitle;

	private void Awake()
	{
		// 버튼 이벤트 연결
		if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
		if (nextButton != null) nextButton.onClick.AddListener(NextPage);
		if (closeButton != null) closeButton.onClick.AddListener(CloseViewer);

		// 시작 시 비활성화
		viewerPanel?.SetActive(false);
	}

	private void Update()
	{
		if (viewerPanel != null && viewerPanel.activeSelf)
		{
			// 좌우 방향키로 페이지 넘기기
			if (Input.GetKeyDown(KeyCode.RightArrow)) NextPage();
			if (Input.GetKeyDown(KeyCode.LeftArrow)) PrevPage();

			// E 또는 ESC로 닫기
			if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
			{
				CloseViewer();
			}
		}
	}

	/// <summary>
	/// 문서 뷰어 열기
	/// </summary>
	public void OpenDocument(string title, Sprite[] pages, string content = "")
	{
		if (pages == null || pages.Length == 0)
		{
			Debug.LogWarning("[DocumentViewer] 페이지 이미지가 없습니다!");
			return;
		}

		_currentTitle = title;
		_currentPages = pages;
		_currentPageIndex = 0;

		viewerPanel?.SetActive(true);

		// 게임 상태 변경 (이동 차단)
		GameManager.Instance?.ChangeState(GameState.Viewer);

		// 커서 활성화
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// 첫 페이지 표시
		UpdatePageDisplay();

		if (!string.IsNullOrEmpty(content) && contentText != null)
		{
			contentText.text = content;
		}
	}

	/// <summary>
	/// 환경 단서 뷰어 열기
	/// </summary>
	public void OpenEnvironment(string title, Sprite image, string hint = "")
	{
		_currentTitle = title;
		_currentPages = new Sprite[] { image };
		_currentPageIndex = 0;

		viewerPanel?.SetActive(true);
		GameManager.Instance?.ChangeState(GameState.Viewer);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		UpdatePageDisplay();

		// 힌트 텍스트 표시
		if (!string.IsNullOrEmpty(hint) && contentText != null)
		{
			contentText.text = hint;
		}
	}

	private void NextPage()
	{
		if (_currentPages == null) return;

		if (_currentPageIndex < _currentPages.Length - 1)
		{
			_currentPageIndex++;
			UpdatePageDisplay();
		}
	}

	private void PrevPage()
	{
		if (_currentPageIndex > 0)
		{
			_currentPageIndex--;
			UpdatePageDisplay();
		}
	}

	private void UpdatePageDisplay()
	{
		// 이미지 업데이트
		if (documentImage != null && _currentPages != null)
		{
			documentImage.sprite = _currentPages[_currentPageIndex];
		}

		// 제목 업데이트
		if (titleText != null)
		{
			titleText.text = _currentTitle;
		}

		// 페이지 표시 업데이트
		if (pageText != null && _currentPages != null)
		{
			if (_currentPages.Length > 1)
			{
				pageText.text = $"{_currentPageIndex + 1} / {_currentPages.Length}";
				pageText.gameObject.SetActive(true);
			}
			else
			{
				pageText.gameObject.SetActive(false);
			}
		}

		// 이전/다음 버튼 상태
		if (prevButton != null)
			prevButton.interactable = _currentPageIndex > 0;

		if (nextButton != null)
			nextButton.interactable = _currentPages != null &&
									  _currentPageIndex < _currentPages.Length - 1;
	}

	public void CloseViewer()
	{
		viewerPanel?.SetActive(false);

		// 게임 상태 복귀
		GameManager.Instance?.ChangeState(GameState.Playing);

		// 커서 다시 잠금
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public bool IsOpen => viewerPanel != null && viewerPanel.activeSelf;
}