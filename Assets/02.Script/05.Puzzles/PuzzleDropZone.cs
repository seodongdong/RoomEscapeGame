using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 월드 스페이스 퍼즐용 드롭존
///
/// [수정]
/// - GetPlacedItem() 추가 → Stage1에서 오답 시 아이템 원위치용
/// </summary>
public class PuzzleDropZone : MonoBehaviour
{
	public interface IDropZonePuzzle
	{
		void OnItemPlacedOnZone(PuzzleDropZone zone);
	}

	[Header("슬롯 식별 (정답 체크용)")]
	[Tooltip("Stage4/5용: 정답 아이템 ID. 비워두면 색상으로 판단.")]
	public string requiredItemId = "";

	[Tooltip("Stage2용: 정답 색상. requiredItemId가 있으면 무시.")]
	public Color requiredColor = Color.white;

	public int slotIndex = 0;

	[Header("여아 사진 스프라이트")]
	[SerializeField] private SpriteRenderer photoSpriteRenderer;
	[SerializeField] private Sprite emptySprite;
	[SerializeField] private Sprite smileSprite;
	[SerializeField] private Sprite bigSmileSprite;

	[Header("사탕 시각 오브젝트 (선택)")]
	[SerializeField] private SpriteRenderer candySpriteRenderer;
	[SerializeField] private Sprite candySprite;

	private IDropZonePuzzle _puzzle;
	private PuzzleDraggableItem _placedItem;

	public bool IsOccupied { get; private set; } = false;

	public bool IsCorrectlyFilled =>
		IsOccupied && _placedItem != null && IsMatchFor(_placedItem);

	public bool IsCorrect => IsCorrectlyFilled;

	public void Initialize(IDropZonePuzzle puzzle)
	{
		_puzzle = puzzle;
		ResetVisuals();
	}

	/// <summary>
	/// 어떤 아이템이든 수락. 놓는 즉시 smileSprite로 바뀜.
	/// 정답 여부는 IsCorrectlyFilled로 확인.
	/// </summary>
	public bool TryAcceptItem(PuzzleDraggableItem item)
	{
		if (IsOccupied) return false;

		IsOccupied = true;
		_placedItem = item;

		SetSprite(smileSprite);
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

	/// <summary>
	/// ★ 추가: 현재 놓인 아이템 반환 (Stage1 오답 시 원위치용)
	/// </summary>
	public PuzzleDraggableItem GetPlacedItem() => _placedItem;

	public void SetBigSmileExpression() => SetSprite(bigSmileSprite);

	public bool IsMatchFor(PuzzleDraggableItem item)
	{
		if (!string.IsNullOrEmpty(requiredItemId))
			return item.itemId == requiredItemId;
		return ApproxColorEqual(item.itemColor, requiredColor);
	}

	// ── 내부 ─────────────────────────────────────────────────

	private void SetSprite(Sprite sprite)
	{
		if (photoSpriteRenderer != null && sprite != null)
			photoSpriteRenderer.sprite = sprite;
	}

	private void ShowCandyVisual(Color color)
	{
		if (candySpriteRenderer == null) return;
		candySpriteRenderer.gameObject.SetActive(true);
		if (candySprite != null) candySpriteRenderer.sprite = candySprite;
		candySpriteRenderer.color = color;
	}

	private void ResetVisuals()
	{
		SetSprite(emptySprite);
		if (candySpriteRenderer != null)
		{
			candySpriteRenderer.gameObject.SetActive(false);
			candySpriteRenderer.color = Color.white;
		}
	}

	private bool ApproxColorEqual(Color a, Color b, float tolerance = 0.05f)
	{
		return Mathf.Abs(a.r - b.r) < tolerance &&
			   Mathf.Abs(a.g - b.g) < tolerance &&
			   Mathf.Abs(a.b - b.b) < tolerance;
	}
}