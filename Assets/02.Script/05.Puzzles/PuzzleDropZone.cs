using UnityEngine;

/// <summary>
/// 월드 스페이스 퍼즐용 드롭존
///
/// [v3 변경사항]
/// TryAcceptItem이 어떤 아이템이든 smileSprite로 바꾸던 것을 고쳤습니다.
/// 기획서: "알맞은 프랍을 해당 공간에 드롭하면 인형이 눈을 뜸 = 정답 /
/// 알맞지 않은 프랍을 두면 인형이 눈을 뜨지 않음"
///
/// 정답일 때만 눈을 뜨므로, 플레이어가 틀린 걸 알아채고 다시 빼낼 수 있습니다.
/// 예전처럼 아무 아이템에나 반응하게 하려면 smileOnlyWhenCorrect를 해제하세요.
///
/// [v2 변경사항]
/// GetPlacedItem() 추가 → Stage1에서 오답 아이템 참조용
/// </summary>
public class PuzzleDropZone : MonoBehaviour
{
	public interface IDropZonePuzzle
	{
		void OnItemPlacedOnZone(PuzzleDropZone zone);
	}

	[Header("슬롯 식별 (정답 체크용)")]
	[Tooltip("정답 아이템 ID. 비워두면 색상으로 판단.")]
	public string requiredItemId = "";

	[Tooltip("정답 색상. requiredItemId가 있으면 무시.")]
	public Color requiredColor = Color.white;

	public int slotIndex = 0;

	[Header("여아 사진 스프라이트")]
	[SerializeField] private SpriteRenderer photoSpriteRenderer;
	[SerializeField] private Sprite emptySprite;
	[SerializeField] private Sprite smileSprite;
	[SerializeField] private Sprite bigSmileSprite;

	[Tooltip("체크하면 정답일 때만 눈을 뜹니다. (기획서 기준)")]
	[SerializeField] private bool smileOnlyWhenCorrect = true;

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
	/// 어떤 아이템이든 자리에 놓을 수는 있습니다.
	/// 다만 인형이 눈을 뜨는 건 정답일 때뿐입니다.
	/// </summary>
	public bool TryAcceptItem(PuzzleDraggableItem item)
	{
		if (IsOccupied) return false;

		IsOccupied = true;
		_placedItem = item;

		bool correct = IsMatchFor(item);

		if (correct || !smileOnlyWhenCorrect)
		{
			SetSprite(smileSprite);
			ShowCandyVisual(item.itemColor);
		}

		_puzzle?.OnItemPlacedOnZone(this);
		return true;
	}

	public void RemoveItem()
	{
		IsOccupied = false;
		_placedItem = null;
		ResetVisuals();
	}

	public PuzzleDraggableItem GetPlacedItem() => _placedItem;

	public void SetBigSmileExpression() => SetSprite(bigSmileSprite);

	public bool IsMatchFor(PuzzleDraggableItem item)
	{
		if (item == null) return false;

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