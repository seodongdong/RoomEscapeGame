using UnityEngine;
using System.Collections;

/// <summary>
/// 환경 오브젝트 상호작용
///
/// [모드]
/// - ViewMode.Viewer      : 오브젝트를 카메라 정면으로 이동해서 돌려보기
/// - ViewMode.DialogueOnly: 대사만 출력 (오브젝트 이동 없음)
///
/// [수정]
/// - 뷰어 열릴 때 UILayerManager.Push → ESC/E로 닫기 가능
/// - 선택적 인벤토리 등록 옵션 추가 (addToInventory = true 시)
/// - 카메라 자식 부착 방식 유지
/// </summary>
public class ObjectViewer3D : MonoBehaviour, IInteractable
{
	public enum ViewMode { Viewer, DialogueOnly }

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
	[SerializeField] private float viewDistance = 0.5f;
	[Tooltip("카메라 중심 기준 상하 오프셋")]
	[SerializeField] private float viewOffsetY = 0f;
	[SerializeField] private float zoomDuration = 0.4f;
	[SerializeField] private float rotateSpeed = 3f;
	[SerializeField] private GameObject viewerHintUI;

	[Header("인벤토리 등록 (선택)")]
	[Tooltip("true: 첫 조사 시 아이템 탭에 등록됨 (살펴보기용 단서)")]
	[SerializeField] private bool addToInventory = false;
	[SerializeField] private string itemDate = "";
	[SerializeField] private GameObject itemPrefab;   // 3D 뷰어용 프리팹 (없으면 자기 자신)

	// ── 원래 상태 저장 ────────────────────────────────────────
	private Vector3 _originalPosition;
	private Quaternion _originalRotation;
	private Vector3 _originalScale;
	private Transform _originalParent;

	// ── 뷰어 상태 ─────────────────────────────────────────────
	private bool _isViewing = false;
	private bool _isRegistered = false;
	private bool _isDragging = false;
	private Vector3 _lastMousePos;

	private Camera _cam;
	private Player _player;

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt => $"[F] {clueName} 조사하기";
	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		if (Stage1TVPriorityManager.CheckPriorityBlocked(player)) return;

		// 첫 조사: 단서 등록
		if (!_isRegistered)
		{
			_isRegistered = true;

			// 클루 트래커 등록 (항상)
			if (!string.IsNullOrEmpty(clueId))
				GameManager.Instance?.ClueTracker.RegisterClue(clueId);

			// ★ 인벤토리 등록 (옵션)
			if (addToInventory && !string.IsNullOrEmpty(clueId))
			{
				// PlayerInventory
				player.Inventory.AddItem(new ClueItem(clueId, clueName, description));

				// InventoryUI_Complete
				var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>(FindObjectsInactive.Include);
				inventoryUI?.AddItem(new InventoryItemData
				{
					itemId = clueId,
					title = clueName,
					date = itemDate,
					itemType = ItemType.UsableItem,
					description = description,
					itemPrefab = itemPrefab != null ? itemPrefab : gameObject
				});
			}
		}

		// DialogueOnly 모드: 대사만 출력하고 끝
		if (viewMode == ViewMode.DialogueOnly)
		{
			FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, dialogue);
			return;
		}

		// Viewer 모드: 이미 보는 중이면 무시
		if (_isViewing) return;

		_player = FindAnyObjectByType<Player>();
		_cam = Camera.main;

		StartCoroutine(ZoomIn());
	}

	// ── Update (뷰어 열려있을 때만) ───────────────────────────

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

		// ★ ESC/E는 UILayerManager가 처리 → 여기서는 제거
		//   (ZoomOut은 UILayerManager의 onClose 콜백으로 호출됨)
	}

	// ── ZoomIn ────────────────────────────────────────────────

	private IEnumerator ZoomIn()
	{
		_isViewing = true;

		if (_player != null) _player.enabled = false;
		GameManager.Instance?.StateManager.ChangeState(GameState.Viewer);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		viewerHintUI?.SetActive(true);

		// ★ UILayerManager에 등록 → ESC/E 눌리면 ZoomOut 호출
		UILayerManager.Instance?.Push(this, () => StartCoroutine(ZoomOut()));

		// 원래 상태 저장
		_originalPosition = transform.position;
		_originalRotation = transform.rotation;
		_originalScale = transform.localScale;
		_originalParent = transform.parent;

		// 카메라 자식으로 부착
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

	// ── ZoomOut ───────────────────────────────────────────────

	private IEnumerator ZoomOut()
	{
		viewerHintUI?.SetActive(false);

		// UILayerManager에서 제거
		UILayerManager.Instance?.Pop(this);

		Vector3 startWorldPos = transform.position;

		// 부모 복원
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