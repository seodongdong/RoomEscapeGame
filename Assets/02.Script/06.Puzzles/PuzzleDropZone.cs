using UnityEngine;

/// <summary>
/// 월드 스페이스 퍼즐용 드롭존 (방석 위 슬롯, 접시 위 재료 자리, 장식장 칸 등).
///
/// [v2 변경사항]
/// - IDropZonePuzzle 인터페이스 내장. Stage2/4/5 컨트롤러가 이 인터페이스를 구현하면
///   Initialize() 한 번으로 어느 스테이지든 연결됩니다.
/// - requiredItemId 필드 추가. 비어있으면 색상 매칭(Stage2), 값이 있으면 ID 매칭(Stage4/5).
/// </summary>
public class PuzzleDropZone : MonoBehaviour
{
	/// <summary>
	/// 드롭존을 사용하는 모든 퍼즐 컨트롤러가 구현해야 하는 인터페이스.
	/// Stage2_AltarCandyPuzzle, Stage4_ToyFoodPuzzle, Stage5_BasementPuzzle이 구현합니다.
	/// </summary>
	public interface IDropZonePuzzle
	{
		void OnItemPlacedOnZone(PuzzleDropZone zone);
	}

	[Header("슬롯 식별 - 둘 중 하나만 설정하면 됩니다")]
	[Tooltip("Stage4/5용: 이 슬롯에 놓을 수 있는 아이템 ID. 비워두면 색상 매칭으로 대체.")]
	public string requiredItemId = "";

	[Tooltip("Stage2용: 이 슬롯에 놓을 수 있는 정답 색상. requiredItemId가 있으면 무시됨.")]
	public Color requiredColor = Color.white;

	[Tooltip("슬롯 인덱스 (퍼즐 컨트롤러에서 진행 상황 로그용)")]
	public int slotIndex = 0;

	[Header("시각 피드백")]
	[SerializeField] private Renderer photoRenderer;
	[SerializeField] private Material emptyMaterial;
	[SerializeField] private Material correctMaterial;
	[SerializeField] private Material bigSmileMaterial;
	[SerializeField] private Renderer candyVisualRenderer;

	// 퍼즐 컨트롤러 (인터페이스로 참조하므로 어느 스테이지든 호환)
	private IDropZonePuzzle _puzzle;

	public bool IsOccupied { get; private set; } = false;
	public bool IsCorrect { get; private set; } = false;

	/// <summary>
	/// Awake()에서 퍼즐 컨트롤러가 호출해서 자신을 등록합니다.
	/// 컨트롤러가 IDropZonePuzzle을 구현하고 있으면 됩니다.
	/// </summary>
	public void Initialize(IDropZonePuzzle puzzle)
	{
		_puzzle = puzzle;
		ResetVisuals();
	}

	/// <summary>
	/// PuzzleDraggableItem.OnMouseUp()에서 호출됩니다.
	/// 정답이면 true, 아니면 false를 반환해서 아이템이 원위치로 돌아가게 합니다.
	/// </summary>
	public bool TryAcceptItem(PuzzleDraggableItem item)
	{
		if (IsOccupied) return false;

		bool matches = IsMatch(item);
		if (!matches)
		{
			Debug.Log($"[DropZone {slotIndex}] 불일치 - 거부");
			return false;
		}

		IsOccupied = true;
		IsCorrect = true;
		UpdatePhotoExpression(false);
		ShowCandyVisual(item.itemColor);

		_puzzle?.OnItemPlacedOnZone(this);
		return true;
	}

	/// <summary>아이템이 다시 집힐 때 또는 리셋 시 슬롯을 비웁니다.</summary>
	public void RemoveItem()
	{
		IsOccupied = false;
		IsCorrect = false;
		ResetVisuals();
	}

	/// <summary>퍼즐 전체 정답 시 마지막 표정으로 업데이트.</summary>
	public void SetBigSmileExpression()
	{
		if (photoRenderer != null && bigSmileMaterial != null)
			photoRenderer.material = bigSmileMaterial;
	}

	// ────────────────────────────────────────────
	// 내부 헬퍼
	// ────────────────────────────────────────────

	/// <summary>
	/// requiredItemId가 있으면 ID 비교, 없으면 색상 비교.
	/// </summary>
	private bool IsMatch(PuzzleDraggableItem item)
	{
		if (!string.IsNullOrEmpty(requiredItemId))
			return item.itemId == requiredItemId;
		else
			return ApproxColorEqual(item.itemColor, requiredColor);
	}

	private void UpdatePhotoExpression(bool isFinal)
	{
		if (photoRenderer == null) return;
		Material mat = isFinal ? bigSmileMaterial : correctMaterial;
		if (mat != null) photoRenderer.material = mat;
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