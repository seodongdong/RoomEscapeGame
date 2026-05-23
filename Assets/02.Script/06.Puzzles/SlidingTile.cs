using UnityEngine;

/// <summary>
/// Stage3 슬라이딩 퍼즐의 개별 타일 컴포넌트.
///
/// [역할]
/// 자신의 "정답 위치"(targetRow, targetCol)를 기억하고,
/// 마우스 클릭 이벤트를 Stage3_SlidingPuzzle에 전달합니다.
/// 퍼즐 매니저가 그리드를 초기화할 때 Initialize()로 정답 위치를 주입합니다.
///
/// [씬 설정]
/// - 타일 모델 오브젝트에 이 스크립트 + Collider(IsTrigger 불필요) 부착
/// - Inspector 연결 불필요 (Initialize()로 런타임에 설정됨)
/// </summary>
public class SlidingTile : MonoBehaviour
{
	// Inspector에서 미리 설정하거나, Initialize()로 런타임 설정 가능
	// (둘 다 안 하면 0,0이 기본값)
	[Tooltip("이 타일의 정답 행 (0부터 시작). Inspector에서 직접 설정해도 됩니다.")]
	public int targetRow = 0;
	[Tooltip("이 타일의 정답 열 (0부터 시작).")]
	public int targetCol = 0;

	private Stage3_SlidingPuzzle _puzzle;
	private bool _isInteractable = false;

	/// <summary>
	/// 퍼즐 매니저가 그리드 초기화 시 호출.
	/// Inspector에서 미리 설정했다면 호출 안 해도 됩니다.
	/// </summary>
	public void Initialize(Stage3_SlidingPuzzle puzzle, int row, int col)
	{
		_puzzle = puzzle;
		targetRow = row;
		targetCol = col;
		_isInteractable = true;
	}

	/// <summary>퍼즐 매니저가 Start() 이후 호출해서 클릭 활성화.</summary>
	public void SetInteractable(Stage3_SlidingPuzzle puzzle)
	{
		_puzzle = puzzle;
		_isInteractable = true;
	}

	private void OnMouseDown()
	{
		if (!_isInteractable) return;
		_puzzle?.TrySlide(this);
	}
}