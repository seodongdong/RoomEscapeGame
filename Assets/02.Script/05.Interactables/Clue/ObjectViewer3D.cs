using UnityEngine;
using System.Collections;

/// <summary>
/// 환경 오브젝트 상호작용
/// - ViewMode.Viewer: 오브젝트를 카메라 정면으로 이동해서 보기 (크기 변경 없음)
/// - ViewMode.DialogueOnly: 대사만 출력
///
/// [수정] 카메라 자식으로 부착 후 로컬 좌표 고정
///        → 오브젝트 위치/거리와 무관하게 항상 카메라 정면에 등장
/// </summary>
public class ObjectViewer3D : MonoBehaviour, IInteractable
{
	public enum ViewMode
	{
		Viewer,
		DialogueOnly
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

	[Header("Viewer Settings")]
	[Tooltip("카메라 앞 몇 미터에 오브젝트를 놓을지 (0.3 ~ 1.0 권장)")]
	[SerializeField] private float viewDistance = 0.5f;     // 카메라 로컬 Z 거리
	[Tooltip("카메라 중심 기준 상하 오프셋 (0이면 정중앙)")]
	[SerializeField] private float viewOffsetY = 0f;        // 카메라 로컬 Y 오프셋
	[SerializeField] private float zoomDuration = 0.4f;
	[SerializeField] private float rotateSpeed = 3f;
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
	private Vector3 _lastMousePos;

	private Camera _cam;
	private Player _player;

	// ── IInteractable ──────────────────────────────────
	public string InteractionPrompt => $"[F] {clueName} 조사하기";
	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		if (Stage1TVPriorityManager.CheckPriorityBlocked(player)) return;

		if (!_isRegistered)
		{
			_isRegistered = true;
			GameManager.Instance?.ClueTracker.RegisterClue(clueId);
		}

		if (viewMode == ViewMode.DialogueOnly)
		{
			FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, dialogue);
			return;
		}

		if (_isViewing) return;

		_player = FindAnyObjectByType<Player>();
		_cam = Camera.main;

		StartCoroutine(ZoomIn());
	}

	// ── Update (뷰어 열려있을 때만) ────────────────────
	private void Update()
	{
		if (!_isViewing) return;

		// 마우스 드래그 → 오브젝트 회전
		if (Input.GetMouseButtonDown(0)) { _isDragging = true; _lastMousePos = Input.mousePosition; }
		if (Input.GetMouseButtonUp(0)) _isDragging = false;

		if (_isDragging)
		{
			Vector3 delta = Input.mousePosition - _lastMousePos;
			transform.Rotate(Vector3.up, -delta.x * rotateSpeed, Space.World);
			transform.Rotate(Vector3.right, delta.y * rotateSpeed, Space.World);
			_lastMousePos = Input.mousePosition;
		}

		if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
			StartCoroutine(ZoomOut());
	}

	// ── ZoomIn ─────────────────────────────────────────
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

		// ⭐ 카메라 자식으로 부착
		//    로컬 좌표 (0, offsetY, viewDistance) = 항상 카메라 정면 고정 위치
		transform.SetParent(_cam.transform);

		Vector3 startLocalPos = transform.localPosition;
		Vector3 targetLocalPos = new Vector3(0f, viewOffsetY, viewDistance);

		float elapsed = 0f;
		while (elapsed < zoomDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration);
			transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, t);
			yield return null;
		}

		transform.localPosition = targetLocalPos;

		// 대사 출력
		if (!string.IsNullOrEmpty(dialogue))
			FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, dialogue);
	}

	// ── ZoomOut ────────────────────────────────────────
	private IEnumerator ZoomOut()
	{
		viewerHintUI?.SetActive(false);

		// 카메라 자식 상태에서 월드 좌표로 현재 위치 기록
		Vector3 startWorldPos = transform.position;

		// 부모를 원래대로 복원
		transform.SetParent(_originalParent);

		float elapsed = 0f;
		while (elapsed < zoomDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration);
			transform.position = Vector3.Lerp(startWorldPos, _originalPosition, t);
			yield return null;
		}

		// 완전 복원
		transform.position = _originalPosition;
		transform.rotation = _originalRotation;
		transform.localScale = _originalScale;

		_isViewing = false;

		if (_player != null) _player.enabled = true;
		GameManager.Instance?.StateManager.ChangeState(GameState.Playing);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}