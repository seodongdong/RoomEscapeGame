using UnityEngine;
using System.Collections;

/// <summary>
/// 환경 오브젝트 상호작용
/// - ViewMode.Viewer: 3D 확대 회전 보기 (기존)
/// - ViewMode.DialogueOnly: 대사만 출력 (커다란 오브젝트용)
/// </summary>
public class ObjectViewer3D : MonoBehaviour, IInteractable
{
	public enum ViewMode
	{
		Viewer,         // 3D 확대 회전
		DialogueOnly    // 대사만 출력
	}

	[Header("모드 선택")]
	[SerializeField] private ViewMode viewMode = ViewMode.Viewer;

	[Header("Clue Info")]
	[SerializeField] private string clueId;
	[SerializeField] private string clueName;
	[TextArea(2, 5)]
	[SerializeField] private string description;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue;

	// ── Viewer 모드 전용 ──────────────────────────
	[Header("Viewer Settings (Viewer 모드만 사용)")]
	[SerializeField] private float zoomDistance = 1.5f;
	[SerializeField] private float zoomDuration = 0.5f;
	[SerializeField] private float rotateSpeed = 3f;
	[SerializeField] private Vector3 viewScale = Vector3.one;
	[SerializeField] private GameObject viewerHintUI;

	// 원래 상태 저장
	private Vector3 _originalPosition;
	private Quaternion _originalRotation;
	private Vector3 _originalScale;
	private Transform _originalParent;

	// 뷰어 상태
	private bool _isViewing = false;
	private bool _isRegistered = false;
	private bool _isDragging = false;
	private Vector3 _lastMousePosition;

	private Camera _playerCamera;
	private Player _player;

	// ── IInteractable ──────────────────────────────
	public string InteractionPrompt => $"[F] {clueName} 조사하기";

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		if (Stage1TVPriorityManager.CheckPriorityBlocked(player)) return;

		// 단서 최초 등록
		if (!_isRegistered)
		{
			_isRegistered = true;
			GameManager.Instance?.ClueTracker.RegisterClue(clueId);
		}

		if (viewMode == ViewMode.DialogueOnly)
		{
			// ── 대사만 출력 ──────────────────────
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue(speaker, dialogue);
		}
		else
		{
			// ── 3D 뷰어 ──────────────────────────
			if (_isViewing) return;

			_player = FindAnyObjectByType<Player>();
			_playerCamera = Camera.main;

			StartCoroutine(ZoomIn());
		}
	}

	// ── Viewer 모드 전용 로직 ──────────────────────

	private void Update()
	{
		if (viewMode != ViewMode.Viewer) return;
		if (!_isViewing) return;

		// 마우스 드래그 회전
		if (Input.GetMouseButtonDown(0))
		{
			_isDragging = true;
			_lastMousePosition = Input.mousePosition;
		}
		if (Input.GetMouseButtonUp(0))
			_isDragging = false;

		if (_isDragging)
		{
			Vector3 delta = Input.mousePosition - _lastMousePosition;
			transform.Rotate(Vector3.up, -delta.x * rotateSpeed, Space.World);
			transform.Rotate(Vector3.right, delta.y * rotateSpeed, Space.World);
			_lastMousePosition = Input.mousePosition;
		}

		// E 또는 ESC로 닫기
		if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
			HandleExit();
	}

	private IEnumerator ZoomIn()
	{
		_isViewing = true;

		if (_player != null) _player.enabled = false;
		GameManager.Instance?.StateManager.ChangeState(GameState.Viewer);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		viewerHintUI?.SetActive(true);

		// 원래 상태 저장
		_originalPosition = transform.position;
		_originalRotation = transform.rotation;
		_originalScale = transform.localScale;
		_originalParent = transform.parent;

		// 카메라 앞으로 이동
		Vector3 targetPos = _playerCamera.transform.position
						  + _playerCamera.transform.forward * zoomDistance;

		float elapsed = 0f;
		while (elapsed < zoomDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = elapsed / zoomDuration;
			transform.position = Vector3.Lerp(_originalPosition, targetPos, t);
			transform.localScale = Vector3.Lerp(_originalScale, viewScale, t);
			yield return null;
		}

		transform.position = targetPos;
		transform.localScale = viewScale;

		// 대사 출력
		if (!string.IsNullOrEmpty(dialogue))
		{
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue(speaker, dialogue);
		}
	}

	private IEnumerator ZoomOut()
	{
		viewerHintUI?.SetActive(false);

		Vector3 startPos = transform.position;
		Vector3 startScale = transform.localScale;

		float elapsed = 0f;
		while (elapsed < zoomDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = elapsed / zoomDuration;
			transform.position = Vector3.Lerp(startPos, _originalPosition, t);
			transform.localScale = Vector3.Lerp(startScale, _originalScale, t);
			yield return null;
		}

		transform.position = _originalPosition;
		transform.rotation = _originalRotation;
		transform.localScale = _originalScale;
		transform.SetParent(_originalParent);

		_isViewing = false;

		if (_player != null) _player.enabled = true;
		GameManager.Instance?.StateManager.ChangeState(GameState.Playing);

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void HandleExit()
	{
		StartCoroutine(ZoomOut());
	}
}