using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 월드 스페이스 퍼즐용 드롭존.
///
/// [v5 변경사항 - SpriteRenderer 방식]
/// Renderer + Material 방식 → SpriteRenderer + Sprite 방식으로 교체.
/// Inspector에서 Material 만들 필요 없이 Sprite를 바로 연결하면 됩니다.
/// 방석 위 오브젝트에 SpriteRenderer 컴포넌트를 붙이고 연결하세요.
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
	[Tooltip("방석 위 사진을 표시하는 SpriteRenderer 컴포넌트")]
	[SerializeField] private SpriteRenderer photoSpriteRenderer;

	[Tooltip("기본 표정 (무표정 / 사탕 없을 때)")]
	[SerializeField] private Sprite emptySprite;

	[Tooltip("웃는 표정 (사탕 놓였을 때, 정답/오답 무관)")]
	[SerializeField] private Sprite smileSprite;

	[Tooltip("활짝 웃는 표정 (마지막 정답 슬롯에만)")]
	[SerializeField] private Sprite bigSmileSprite;

	[Header("사탕 시각 오브젝트 (선택)")]
	[Tooltip("사탕이 올려졌을 때 표시될 SpriteRenderer. 없어도 됩니다.")]
	[SerializeField] private SpriteRenderer candySpriteRenderer;

	[Tooltip("사탕 기본 스프라이트 (색상으로 구분할 것이므로 흰색 원 하나면 됩니다)")]
	[SerializeField] private Sprite candySprite;

	private IDropZonePuzzle _puzzle;
	private PuzzleDraggableItem _placedItem;

	public bool IsOccupied { get; private set; } = false;

	/// <summary>올바른 아이템이 놓여있는지 (정답 체크용)</summary>
	public bool IsCorrectlyFilled =>
		IsOccupied && _placedItem != null && IsMatchFor(_placedItem);

	/// <summary>하위 호환용 — IsCorrectlyFilled와 동일</summary>
	public bool IsCorrect => IsCorrectlyFilled;

	public void Initialize(IDropZonePuzzle puzzle)
	{
		_puzzle = puzzle;
		ResetVisuals();
	}

	/// <summary>
	/// 어떤 아이템이든 수락. 놓는 즉시 웃는 표정으로 바뀝니다.
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

	/// <summary>마지막 정답 슬롯에만 활짝 웃는 표정.</summary>
	public void SetBigSmileExpression()
	{
		SetSprite(bigSmileSprite);
	}

	/// <summary>
	/// 이 아이템이 이 존의 정답인지 확인.
	/// requiredItemId 있으면 ID 비교, 없으면 색상 비교.
	/// </summary>
	public bool IsMatchFor(PuzzleDraggableItem item)
	{
		if (!string.IsNullOrEmpty(requiredItemId))
			return item.itemId == requiredItemId;
		return ApproxColorEqual(item.itemColor, requiredColor);
	}

	// ── 내부 헬퍼 ─────────────────────────────────────────────

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