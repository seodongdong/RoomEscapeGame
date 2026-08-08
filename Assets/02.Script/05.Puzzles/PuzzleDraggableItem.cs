using UnityEngine;

/// <summary>
/// 월드 스페이스 퍼즐용 드래그 가능한 아이템.
///
/// [v7 변경사항]
/// 1. ScreenPlane 모드 추가 — 카메라 시선에 수직인 평면을 써서 어떤 카메라
///    각도에서도 마우스와 1:1로 따라옵니다. VerticalFacingCamera는 카메라가
///    비스듬할 때 Y가 거의 안 움직이는 문제가 있어 대체용으로 만들었습니다.
/// 2. Lock() / Unlock() 추가 — 정답 자리에 들어간 조각을 고정합니다.
///    잠긴 조각은 클릭해도 반응하지 않아 실수로 빼내지 않습니다.
/// 3. 잡은 지점 기준 추종 — 모서리를 잡아도 조각이 커서로 순간이동하지 않습니다.
///
/// EnableDragging(cam, surfaceY) 시그니처는 그대로라 기존 퍼즐 코드는
/// 수정 없이 동작합니다.
/// </summary>
public class PuzzleDraggableItem : MonoBehaviour
{
	public enum DragMode
	{
		Horizontal,           // XZ 평면 (Y 고정) — 내려다보는 퍼즐
		ScreenPlane,          // 화면에 수직 — 어떤 각도에서도 1:1 (권장)
		VerticalFacingCamera  // 수직 평면, 깊이 고정 — 카메라가 거의 수평일 때만
	}

	[Header("아이템 식별")]
	public string itemId = "";
	public Color itemColor = Color.white;

	[Header("드래그 설정")]
	[Tooltip("인형의 집처럼 세워진 퍼즐은 ScreenPlane, " +
			 "도마처럼 내려다보는 퍼즐은 Horizontal.")]
	[SerializeField] private DragMode dragMode = DragMode.Horizontal;

	[Tooltip("Horizontal 모드에서 드래그 중 바닥에서 띄우는 높이.")]
	[SerializeField] private float liftHeight = 0.08f;

	[Tooltip("드롭존에 스냅될 때 카메라 쪽으로 당기는 거리. " +
			 "조각이 프랍에 파묻히면 올려보세요.")]
	[SerializeField] private float snapForwardOffset = 0f;

	[Tooltip("이 거리 안에 빈 드롭존이 있으면 스냅됩니다.")]
	[SerializeField] private float snapDistance = 0.6f;

	[Header("이동 범위 제한 (선택)")]
	[Tooltip("체크하면 드래그 영역을 상자로 제한합니다. 조각이 멀리 날아가는 걸 막습니다.")]
	[SerializeField] private bool clampToBounds = false;
	[SerializeField] private Vector3 boundsCenter = Vector3.zero;
	[SerializeField] private Vector3 boundsSize = new Vector3(2f, 2f, 2f);

	[Header("시각 피드백 (선택)")]
	[SerializeField] private Material dragMaterial;
	[SerializeField] private Material defaultMaterial;
	[SerializeField] private Renderer itemRenderer;

	// ── 런타임 ────────────────────────────────────────────────
	private Vector3 _homePosition;
	private bool _homePositionSet = false;
	private float _dragPlaneY;
	private bool _isDragging = false;
	private bool _isDraggingEnabled = false;
	private bool _isLocked = false;
	private PuzzleDropZone _currentZone;
	private Camera _puzzleCamera;

	private Plane _activePlane;
	private Vector3 _grabOffset;

	// ── 외부 API ──────────────────────────────────────────────

	public bool IsLocked => _isLocked;
	public PuzzleDropZone CurrentZone => _currentZone;

	public void EnableDragging(Camera cam, float surfaceY)
	{
		_puzzleCamera = cam;
		_dragPlaneY = surfaceY;
		_isDraggingEnabled = true;

		if (!_homePositionSet)
		{
			_homePosition = transform.position;
			_homePositionSet = true;
		}
	}

	public void DisableDragging()
	{
		_isDraggingEnabled = false;
		_isDragging = false;
		RestoreDefaultMaterial();
	}

	/// <summary>정답 자리에 들어간 조각 고정. 클릭해도 반응하지 않습니다.</summary>
	public void Lock()
	{
		_isLocked = true;
		_isDragging = false;
		RestoreDefaultMaterial();
	}

	public void Unlock() => _isLocked = false;

	public void ResetToHomePosition()
	{
		_isLocked = false;

		if (_currentZone != null) { _currentZone.RemoveItem(); _currentZone = null; }
		if (_homePositionSet) transform.position = _homePosition;

		RestoreDefaultMaterial();
	}

	public void ResetToOriginalPosition() => ResetToHomePosition();

	/// <summary>스폰 직후 등, 홈 위치를 현재 자리로 다시 잡을 때.</summary>
	public void SetHomePositionToCurrent()
	{
		_homePosition = transform.position;
		_homePositionSet = true;
	}

	public void SetDragMode(DragMode mode) => dragMode = mode;

	// ── 드래그 ────────────────────────────────────────────────

	private void OnMouseDown()
	{
		if (!_isDraggingEnabled || _isLocked) return;
		if (_puzzleCamera == null) return;

		_isDragging = true;

		// 드롭존에서 빼내기 — 틀린 자리에 놓았어도 다시 집을 수 있습니다
		if (_currentZone != null) { _currentZone.RemoveItem(); _currentZone = null; }

		ApplyDragMaterial();

		_dragPlaneY = transform.position.y;
		_activePlane = BuildDragPlane();

		// 잡은 지점 기준으로 따라오게 — 모서리를 잡아도 튀지 않습니다
		Ray ray = _puzzleCamera.ScreenPointToRay(Input.mousePosition);
		_grabOffset = _activePlane.Raycast(ray, out float dist)
			? transform.position - ray.GetPoint(dist)
			: Vector3.zero;
	}

	private void OnMouseDrag()
	{
		if (!_isDragging || _puzzleCamera == null) return;

		Ray ray = _puzzleCamera.ScreenPointToRay(Input.mousePosition);
		if (!_activePlane.Raycast(ray, out float distance)) return;

		Vector3 pos = ray.GetPoint(distance) + _grabOffset;

		if (dragMode == DragMode.Horizontal)
			pos.y = _dragPlaneY + liftHeight;

		if (clampToBounds)
			pos = ClampPosition(pos);

		transform.position = pos;
	}

	private void OnMouseUp()
	{
		if (!_isDragging) return;
		_isDragging = false;

		PuzzleDropZone nearest = FindNearestAvailableDropZone();

		if (nearest != null && nearest.TryAcceptItem(this))
		{
			_currentZone = nearest;
			transform.position = GetSnapPosition(nearest);
		}
		// 근처에 빈 존이 없으면 그 자리에 그냥 놔둠

		RestoreDefaultMaterial();
	}

	// ── 평면 계산 ─────────────────────────────────────────────

	private Plane BuildDragPlane()
	{
		Vector3 point = transform.position;

		switch (dragMode)
		{
			case DragMode.ScreenPlane:
				// 시선에 정확히 수직 → 레이가 항상 수직으로 꽂혀 1:1 추종
				return new Plane(_puzzleCamera.transform.forward, point);

			case DragMode.VerticalFacingCamera:
				{
					Vector3 normal = _puzzleCamera.transform.forward;
					normal.y = 0f;

					// 카메라가 거의 수직으로 내려다보면 이 모드는 성립하지 않습니다.
					// 드래그가 멈추는 대신 ScreenPlane으로 자동 대체합니다.
					if (normal.sqrMagnitude < 0.05f)
						return new Plane(_puzzleCamera.transform.forward, point);

					return new Plane(normal.normalized, point);
				}

			case DragMode.Horizontal:
			default:
				return new Plane(Vector3.up, new Vector3(0f, _dragPlaneY, 0f));
		}
	}

	private Vector3 GetSnapPosition(PuzzleDropZone zone)
	{
		Vector3 basePos = zone.transform.position;

		if (dragMode == DragMode.Horizontal)
			return basePos + Vector3.up * liftHeight;

		if (snapForwardOffset != 0f && _puzzleCamera != null)
		{
			Vector3 toCamera = _puzzleCamera.transform.position - basePos;
			if (toCamera.sqrMagnitude > 0.0001f)
				basePos += toCamera.normalized * snapForwardOffset;
		}

		return basePos;
	}

	private Vector3 ClampPosition(Vector3 pos)
	{
		Vector3 min = boundsCenter - boundsSize * 0.5f;
		Vector3 max = boundsCenter + boundsSize * 0.5f;

		return new Vector3(
			Mathf.Clamp(pos.x, min.x, max.x),
			Mathf.Clamp(pos.y, min.y, max.y),
			Mathf.Clamp(pos.z, min.z, max.z));
	}

	// ── 드롭존 탐색 ───────────────────────────────────────────

	private PuzzleDropZone FindNearestAvailableDropZone()
	{
		PuzzleDropZone[] all = FindObjectsByType<PuzzleDropZone>(FindObjectsSortMode.None);
		PuzzleDropZone nearest = null;
		float minDist = snapDistance;

		foreach (var zone in all)
		{
			if (zone.IsOccupied) continue;
			float d = Vector3.Distance(transform.position, zone.transform.position);
			if (d < minDist) { minDist = d; nearest = zone; }
		}
		return nearest;
	}

	// ── 머티리얼 피드백 ───────────────────────────────────────

	private void ApplyDragMaterial()
	{
		if (itemRenderer != null && dragMaterial != null)
			itemRenderer.material = dragMaterial;
	}

	private void RestoreDefaultMaterial()
	{
		if (itemRenderer != null && defaultMaterial != null)
			itemRenderer.material = defaultMaterial;
	}

	// ── 기즈모 ────────────────────────────────────────────────

	private void OnDrawGizmosSelected()
	{
		if (clampToBounds)
		{
			Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.4f);
			Gizmos.DrawWireCube(boundsCenter, boundsSize);
		}

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, snapDistance);
	}
}