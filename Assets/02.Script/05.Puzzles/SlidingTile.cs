using UnityEngine;

/// <summary>
/// Stage3 슬라이딩 퍼즐의 개별 타일 컴포넌트.
///
/// [역할]
/// 자신의 "정답 위치"(targetRow, targetCol)를 기억하고,
/// 마우스 클릭 이벤트를 Stage3_SlidingPuzzle에 전달합니다.
///
/// [이번 수정]
/// ApplyPicture() 추가 — 그림 하나(텍스처)를 gridSize x gridSize로 잘라
/// 자기 위치에 해당하는 조각만 표시합니다. 그림이 4개 순차 진행되는
/// 기획서 구조를 지원하기 위한 것으로, 타일 오브젝트를 그림마다
/// 새로 만들 필요가 없습니다.
///
/// [씬 설정]
/// - 타일 모델 오브젝트에 이 스크립트 + Collider 부착
/// - tileRenderer를 비워두면 자식에서 자동으로 찾습니다
/// - 타일 머티리얼은 각 타일마다 인스턴스가 생기므로 공유 머티리얼을 써도 됩니다
/// </summary>
public class SlidingTile : MonoBehaviour
{
	[Tooltip("이 타일의 정답 행 (0부터 시작). Initialize()로 런타임 설정됩니다.")]
	public int targetRow = 0;
	[Tooltip("이 타일의 정답 열 (0부터 시작).")]
	public int targetCol = 0;

	[Header("그림 표시")]
	[Tooltip("그림 조각이 그려질 렌더러. 비우면 자식에서 자동 탐색합니다.")]
	[SerializeField] private Renderer tileRenderer;

	private Stage3_SlidingPuzzle _puzzle;
	private bool _isInteractable = false;

	private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
	private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

	private void Awake()
	{
		if (tileRenderer == null)
			tileRenderer = GetComponentInChildren<Renderer>();
	}

	/// <summary>퍼즐 매니저가 그리드 초기화 시 호출.</summary>
	public void Initialize(Stage3_SlidingPuzzle puzzle, int row, int col)
	{
		_puzzle = puzzle;
		targetRow = row;
		targetCol = col;
		_isInteractable = true;
	}

	public void SetInteractable(bool interactable) => _isInteractable = interactable;

	/// <summary>
	/// 그림 텍스처를 gridSize 등분해서 자기 위치 조각만 표시합니다.
	/// targetRow / targetCol이 정해진 뒤에 호출해야 합니다.
	/// </summary>
	public void ApplyPicture(Texture picture, int gridSize)
	{
		if (tileRenderer == null || picture == null || gridSize <= 0) return;

		float step = 1f / gridSize;
		Vector2 scale = new Vector2(step, step);
		// UV는 아래에서 위로 증가하므로 행을 뒤집습니다.
		Vector2 offset = new Vector2(targetCol * step, (gridSize - 1 - targetRow) * step);

		Material mat = tileRenderer.material; // 인스턴스화됨 (타일마다 개별 UV)

		if (mat.HasProperty(BaseMapId))
		{
			mat.SetTexture(BaseMapId, picture);
			mat.SetTextureScale(BaseMapId, scale);
			mat.SetTextureOffset(BaseMapId, offset);
		}

		if (mat.HasProperty(MainTexId))
		{
			mat.SetTexture(MainTexId, picture);
			mat.SetTextureScale(MainTexId, scale);
			mat.SetTextureOffset(MainTexId, offset);
		}
	}

	private void OnMouseDown()
	{
		if (!_isInteractable) return;
		if (GameManager.Instance != null &&
			GameManager.Instance.CurrentState != GameState.Puzzle) return;

		_puzzle?.TrySlide(this);
	}
}