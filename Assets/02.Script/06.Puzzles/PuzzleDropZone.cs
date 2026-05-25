using UnityEngine;

/// <summary>
/// 월드 스페이스 퍼즐용 드롭존.
///
/// [v4 변경사항]
/// TryAcceptItem() — 어떤 아이템이든 수락. 놓는 순간 웃는 표정으로 바뀜.
/// _placedItem 필드 추가 — 퍼즐 컨트롤러가 정답 체크 시 사용.
/// IsCorrectlyFilled — 이 존에 올바른 아이템이 놓여있는지 외부에서 확인.
/// </summary>
public class PuzzleDropZone : MonoBehaviour
{
	public interface IDropZonePuzzle
	{
		void OnItemPlacedOnZone(PuzzleDropZone zone);
	}

	[Header("슬롯 식별 (정답 체크용)")]
	[Tooltip("Stage4/5용: 정답 아이템 ID")]
	public string requiredItemId = "";

	[Tooltip("Stage2용: 정답 색상")]
	public Color requiredColor = Color.white;

	public int slotIndex = 0;

	[Header("시각 피드백")]
	[SerializeField] private Renderer photoRenderer;
	[SerializeField] private Material emptyMaterial;      // 아무것도 없을 때
	[SerializeField] private Material smileMaterial;      // 사탕 놓였을 때 (정답/오답 무관)
	[SerializeField] private Material bigSmileMaterial;   // 마지막 정답일 때만

	[SerializeField] private Renderer candyVisualRenderer;

	private IDropZonePuzzle _puzzle;
	private PuzzleDraggableItem _placedItem; // 현재 올려진 아이템

	public bool IsOccupied { get; private set; } = false;

	/// <summary>이 존에 올바른 아이템이 놓여있는지 (정답 체크용)</summary>
	public bool IsCorrectlyFilled =>
		IsOccupied && _placedItem != null && IsMatchFor(_placedItem);

	public void Initialize(IDropZonePuzzle puzzle)
	{
		_puzzle = puzzle;
		ResetVisuals();
	}

	/// <summary>
	/// 어떤 아이템이든 수락. 놓는 즉시 웃는 표정으로 바뀝니다.
	/// 정답 여부는 퍼즐 컨트롤러가 별도로 판단합니다.
	/// </summary>
	public bool TryAcceptItem(PuzzleDraggableItem item)
	{
		if (IsOccupied) return false;

		IsOccupied = true;
		_placedItem = item;

		// 놓는 순간 무조건 웃는 표정
		if (photoRenderer != null && smileMaterial != null)
			photoRenderer.material = smileMaterial;

		ShowCandyVisual(item.itemColor);

		_puzzle?.OnItemPlacedOnZone(this);
		return true;
	}

	public void RemoveItem()
	{
		IsOccupied = false;
		_placedItem = null;
		ResetVisuals();
	}

	/// <summary>마지막 정답 슬롯에만 활짝 웃는 표정 적용.</summary>
	public void SetBigSmileExpression()
	{
		if (photoRenderer != null && bigSmileMaterial != null)
			photoRenderer.material = bigSmileMaterial;
	}

	/// <summary>
	/// 이 아이템이 이 존의 정답인지 확인.
	/// requiredItemId가 있으면 ID 비교, 없으면 색상 비교.
	/// </summary>
	public bool IsMatchFor(PuzzleDraggableItem item)
	{
		if (!string.IsNullOrEmpty(requiredItemId))
			return item.itemId == requiredItemId;
		return ApproxColorEqual(item.itemColor, requiredColor);
	}

	private void ShowCandyVisual(Color color)
	{
		if (candyVisualRenderer == null) return;
		candyVisualRenderer.gameObject.SetActive(true);
		candyVisualRenderer.material.color = color;
	}

	private void ResetVisuals()
	{
		if (photoRenderer != null && emptyMaterial != null)
			photoRenderer.material = emptyMaterial;
		if (candyVisualRenderer != null)
			candyVisualRenderer.gameObject.SetActive(false);
	}

	private bool ApproxColorEqual(Color a, Color b, float tolerance = 0.05f)
	{
		return Mathf.Abs(a.r - b.r) < tolerance &&
			   Mathf.Abs(a.g - b.g) < tolerance &&
			   Mathf.Abs(a.b - b.b) < tolerance;
	}
}