using UnityEngine;

/// <summary>
/// 월드 스페이스 퍼즐용 드래그 가능한 아이템.
/// 사탕(Stage2), 요리 재료(Stage4), 목각인형(Stage5) 등 모든 퍼즐 아이템에 사용.
///
/// [v2 변경사항]
/// itemId 필드 추가. Stage2처럼 색상 매칭이 필요하면 itemColor를 쓰고,
/// Stage4/5처럼 종류 매칭이 필요하면 itemId를 쓰면 됩니다.
/// PuzzleDropZone이 requiredItemId가 비어있으면 색상으로, 있으면 ID로 판단합니다.
/// </summary>
public class PuzzleDraggableItem : MonoBehaviour
{
	[Header("아이템 식별 - 둘 중 하나만 써도 됩니다")]
	[Tooltip("Stage4/5처럼 종류 기반 매칭. 비워두면 색상 매칭으로 대체됩니다.")]
	public string itemId = "";

	[Tooltip("Stage2처럼 색상 기반 매칭. itemId가 설정된 경우 우선순위 낮음.")]
	public Color itemColor = Color.white;

	[Header("드래그 설정")]
	[SerializeField] private float liftHeight = 0.08f;
	[SerializeField] private float snapDistance = 0.6f;

	[Header("시각 피드백 (선택)")]
	[SerializeField] private Material dragMaterial;
	[SerializeField] private Material defaultMaterial;
	[SerializeField] private Renderer itemRenderer;

	// 런타임 상태
	private Vector3 _originalPosition;
	private float _dragPlaneY;
	private bool _isDragging = false;
	private bool _isDraggingEnabled = false;
	private PuzzleDropZone _currentZone;
	private Camera _puzzleCamera;

	/// <summary>퍼즐 시작 시 컨트롤러가 호출. 이 순간부터 드래그 가능.</summary>
	public void EnableDragging(Camera cam, float surfaceY)
	{
		_puzzleCamera = cam;
		_dragPlaneY = surfaceY;
		_isDraggingEnabled = true;
		_originalPosition = transform.position;
	}

	/// <summary>퍼즐 종료/해결 시 호출.</summary>
	public void DisableDragging()
	{
		_isDraggingEnabled = false;
		_isDragging = false;
		RestoreDefaultMaterial();
	}

	/// <summary>리셋 시 원래 위치로 강제 복귀.</summary>
	public void ResetToOriginalPosition()
	{
		if (_currentZone != null) { _currentZone.RemoveItem(); _currentZone = null; }
		transform.position = _originalPosition;
		RestoreDefaultMaterial();
	}

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
			Vector3 targetPos = ray.GetPoint(distance);
			targetPos.y = _dragPlaneY + liftHeight;
			transform.position = targetPos;
		}
	}

	private void OnMouseUp()
	{
		if (!_isDragging) return;
		_isDragging = false;
		PuzzleDropZone nearest = FindNearestAvailableDropZone();
		if (nearest != null && nearest.TryAcceptItem(this))
		{
			_currentZone = nearest;
			transform.position = nearest.transform.position + Vector3.up * liftHeight;
		}
		else
		{
			transform.position = _originalPosition;
		}
		RestoreDefaultMaterial();
	}

	private PuzzleDropZone FindNearestAvailableDropZone()
	{
		PuzzleDropZone[] allZones = FindObjectsByType<PuzzleDropZone>(FindObjectsSortMode.None);
		PuzzleDropZone nearest = null;
		float minDist = snapDistance;
		foreach (var zone in allZones)
		{
			if (zone.IsOccupied) continue;
			float dist = Vector3.Distance(transform.position, zone.transform.position);
			if (dist < minDist) { minDist = dist; nearest = zone; }
		}
		return nearest;
	}

	private void ApplyDragMaterial()
	{
		if (itemRenderer != null && dragMaterial != null) itemRenderer.material = dragMaterial;
	}
	private void RestoreDefaultMaterial()
	{
		if (itemRenderer != null && defaultMaterial != null) itemRenderer.material = defaultMaterial;
	}
}