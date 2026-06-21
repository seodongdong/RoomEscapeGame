using UnityEngine;

/// <summary>
/// 월드 스페이스 퍼즐용 드래그 가능한 아이템.
///
/// [v5 변경사항]
/// FindNearestAvailableDropZone() — 정답 여부 무관하게 가장 가까운 빈 존으로 스냅.
/// 어느 방석에나 놓을 수 있습니다.
/// </summary>
public class PuzzleDraggableItem : MonoBehaviour
{
	[Header("아이템 식별")]
	public string itemId = "";
	public Color itemColor = Color.white;

	[Header("드래그 설정")]
	[SerializeField] private float liftHeight = 0.08f;
	[SerializeField] private float snapDistance = 0.6f;

	[Header("시각 피드백 (선택)")]
	[SerializeField] private Material dragMaterial;
	[SerializeField] private Material defaultMaterial;
	[SerializeField] private Renderer itemRenderer;

	private Vector3 _homePosition;
	private bool _homePositionSet = false;
	private float _dragPlaneY;
	private bool _isDragging = false;
	private bool _isDraggingEnabled = false;
	private PuzzleDropZone _currentZone;
	private Camera _puzzleCamera;

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

	public void ResetToHomePosition()
	{
		if (_currentZone != null) { _currentZone.RemoveItem(); _currentZone = null; }
		if (_homePositionSet) transform.position = _homePosition;
		RestoreDefaultMaterial();
	}

	public void ResetToOriginalPosition() => ResetToHomePosition();

	private void OnMouseDown()
	{
		if (!_isDraggingEnabled) return;
		_isDragging = true;
		if (_currentZone != null) { _currentZone.RemoveItem(); _currentZone = null; }
		ApplyDragMaterial();
		_dragPlaneY = transform.position.y;
	}

	private void OnMouseDrag()
	{
		if (!_isDragging || _puzzleCamera == null) return;
		Ray ray = _puzzleCamera.ScreenPointToRay(Input.mousePosition);
		Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, _dragPlaneY, 0f));
		if (dragPlane.Raycast(ray, out float distance))
		{
			Vector3 pos = ray.GetPoint(distance);
			pos.y = _dragPlaneY + liftHeight;
			transform.position = pos;
		}
	}

	private void OnMouseUp()
	{
		if (!_isDragging) return;
		_isDragging = false;

		// 정답 무관하게 가장 가까운 빈 존 탐색
		PuzzleDropZone nearest = FindNearestAvailableDropZone();

		if (nearest != null && nearest.TryAcceptItem(this))
		{
			_currentZone = nearest;
			transform.position = nearest.transform.position + Vector3.up * liftHeight;
		}
		// 근처에 빈 존이 없으면 그 자리에 그냥 놔둠

		RestoreDefaultMaterial();
	}

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
}