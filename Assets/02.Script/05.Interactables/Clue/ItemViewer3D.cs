using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 인벤토리용 3D 아이템 뷰어
///
/// ObjectViewer3D(환경 오브젝트 확대 방식)와 달리,
/// 이 스크립트는 인벤토리의 "보기" 버튼에서 호출되어
/// 별도 뷰어 패널 안에 프리팹 인스턴스를 띄워서 보여줍니다.
///
/// 핵심 차이점:
/// - ObjectViewer3D: 월드에 놓인 오브젝트를 카메라 앞으로 이동
/// - ItemViewer3D:   별도 카메라 + 인스턴스 생성 방식 (안정적)
///
/// [사용법]
/// 1. 빈 GameObject에 ItemViewer3D 컴포넌트 추가
/// 2. Inspector에서 viewerPanel, viewerCamera 등 연결
/// 3. InventoryUI_Complete의 itemViewer3D 슬롯에 연결
///
/// [Unity 씬 세팅]
/// ViewerPanel (Canvas 안)
/// └── ViewerBackground (Image - 어두운 배경)
///     ├── ItemNameText (TMP)
///     ├── HintText (TMP - "마우스 드래그로 회전 | E / 뒤로가기 버튼으로 닫기")
///     └── CloseButton (Button)
/// ViewerCamera (별도 Camera - Culling Mask: ItemViewer 레이어만)
/// ViewerItemRoot (빈 Transform - 아이템 스폰 위치)
/// </summary>
public class ItemViewer3D : MonoBehaviour
{
	[Header("UI")]
	[SerializeField] private GameObject viewerPanel;
	[SerializeField] private TextMeshProUGUI itemNameText;
	[SerializeField] private Button closeButton;

	[Header("Viewer Camera & Item")]
	[SerializeField] private Camera viewerCamera;           // 뷰어 전용 카메라 (레이어: ItemViewer)
	[SerializeField] private Transform itemRoot;            // 아이템 스폰 위치
	[SerializeField] private float itemDisplayDistance = 2f; // 카메라로부터 거리

	[Header("Rotation")]
	[SerializeField] private float rotateSpeed = 200f;
	[SerializeField] private bool autoRotate = true;
	[SerializeField] private float autoRotateSpeed = 30f;

	[Header("Item Layer")]
	[SerializeField] private string itemLayerName = "ItemViewer"; // Project Settings에서 추가 필요

	// 런타임 상태
	private GameObject _currentItem;
	private bool _isDragging;
	private Vector2 _lastMousePos;
	private InventoryUI_Complete _inventoryUI; // 닫을 때 복귀할 인벤토리

	private void Awake()
	{
		if (viewerPanel != null)
			viewerPanel.SetActive(false);

		if (viewerCamera != null)
			viewerCamera.gameObject.SetActive(false);

		closeButton?.onClick.AddListener(CloseViewer);
	}

	// ─────────────────────────────────────────────
	// 열기
	// ─────────────────────────────────────────────

	/// <summary>
	/// 뷰어 열기
	/// </summary>
	/// <param name="prefab">표시할 3D 프리팹</param>
	/// <param name="itemName">아이템 이름</param>
	/// <param name="inventoryUI">닫을 때 복귀할 인벤토리 (null이면 게임으로 복귀)</param>
	public void OpenViewer(GameObject prefab, string itemName, InventoryUI_Complete inventoryUI = null)
	{
		if (prefab == null)
		{
			Debug.LogWarning("[ItemViewer3D] 프리팹이 null입니다.");
			return;
		}

		_inventoryUI = inventoryUI;

		// 기존 아이템 제거
		ClearCurrentItem();

		// 아이템 인스턴스 생성
		_currentItem = Instantiate(prefab);
		_currentItem.transform.SetParent(itemRoot, false);
		_currentItem.transform.localPosition = Vector3.zero;
		_currentItem.transform.localRotation = Quaternion.identity;

		// ItemViewer 레이어 적용 (뷰어 카메라에만 렌더링)
		int layer = LayerMask.NameToLayer(itemLayerName);
		if (layer >= 0)
			SetLayerRecursively(_currentItem, layer);
		else
			Debug.LogWarning($"[ItemViewer3D] '{itemLayerName}' 레이어가 없습니다. Project Settings > Tags and Layers에서 추가하세요.");

		// 콜라이더 비활성화 (클릭 방해 방지)
		foreach (var col in _currentItem.GetComponentsInChildren<Collider>())
			col.enabled = false;

		// UI 설정
		if (itemNameText != null)
			itemNameText.text = itemName;

		viewerPanel?.SetActive(true);

		if (viewerCamera != null)
		{
			viewerCamera.gameObject.SetActive(true);
			// 뷰어 카메라가 ItemViewer 레이어만 렌더링하도록
			if (layer >= 0)
				viewerCamera.cullingMask = 1 << layer;
		}

		Debug.Log($"[ItemViewer3D] 뷰어 열림: {itemName}");
	}

	// ─────────────────────────────────────────────
	// 닫기
	// ─────────────────────────────────────────────

	public void CloseViewer()
	{
		ClearCurrentItem();

		viewerPanel?.SetActive(false);

		if (viewerCamera != null)
			viewerCamera.gameObject.SetActive(false);

		_isDragging = false;

		// 인벤토리로 복귀 or 게임으로 복귀
		if (_inventoryUI != null)
		{
			_inventoryUI.OpenInventory();
			Debug.Log("[ItemViewer3D] 뷰어 닫힘 → 인벤토리 복귀");
		}
		else
		{
			// 직접 게임 복귀
			var player = FindAnyObjectByType<Player>();
			if (player != null) player.enabled = true;

			GameManager.Instance?.StateManager.ChangeState(GameState.Playing);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Debug.Log("[ItemViewer3D] 뷰어 닫힘 → 게임 복귀");
		}
	}

	private void ClearCurrentItem()
	{
		if (_currentItem != null)
		{
			Destroy(_currentItem);
			_currentItem = null;
		}
	}

	// ─────────────────────────────────────────────
	// 업데이트 (회전 처리)
	// ─────────────────────────────────────────────

	private void Update()
	{
		if (viewerPanel == null || !viewerPanel.activeSelf || _currentItem == null)
			return;

		HandleRotation();

		// E키로 닫기
		if (Input.GetKeyDown(KeyCode.E))
			CloseViewer();
	}

	private void HandleRotation()
	{
		// 마우스 드래그 회전
		if (Input.GetMouseButtonDown(0))
		{
			_isDragging = true;
			_lastMousePos = Input.mousePosition;
		}

		if (Input.GetMouseButtonUp(0))
			_isDragging = false;

		if (_isDragging)
		{
			Vector2 delta = (Vector2)Input.mousePosition - _lastMousePos;

			// 좌우 → Y축 회전 / 상하 → X축 회전
			_currentItem.transform.Rotate(Vector3.up, -delta.x * rotateSpeed * Time.deltaTime, Space.World);
			_currentItem.transform.Rotate(Vector3.right, delta.y * rotateSpeed * Time.deltaTime, Space.World);

			_lastMousePos = Input.mousePosition;
		}
		else if (autoRotate)
		{
			// 드래그 안 할 때 자동 회전
			_currentItem.transform.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime, Space.World);
		}
	}

	// ─────────────────────────────────────────────
	// 유틸
	// ─────────────────────────────────────────────

	private void SetLayerRecursively(GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform child in obj.transform)
			SetLayerRecursively(child.gameObject, layer);
	}
}