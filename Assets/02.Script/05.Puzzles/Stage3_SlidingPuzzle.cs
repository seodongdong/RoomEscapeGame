using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 3스테이지 슬라이딩 퍼즐 - 월드 스페이스 클릭 이동 버전
///
/// [동작 원리]
/// 3x3 그리드(8개 타일 + 빈칸 1개)로 구성된 슬라이딩 퍼즐입니다.
/// 플레이어가 타일을 클릭하면 빈칸과 인접한 경우 해당 방향으로 슬라이드됩니다.
/// 모든 타일이 정답 위치에 오면 인형 조각 아이템을 지급하고 퍼즐이 완료됩니다.
///
/// 미로 안에 이 퍼즐이 4개 놓여있고 각각 다른 인형 조각(머리/몸/팔/다리)을 줍니다.
/// 4개를 모두 풀면 출구의 조립 테이블에서 인형을 완성할 수 있습니다.
///
/// [씬 설정]
/// 1. 빈 오브젝트에 이 스크립트 부착
/// 2. gridOrigin: 그리드의 왼쪽-앞 기준점 오브젝트 연결
/// 3. tiles: 씬에 배치된 SlidingTile 오브젝트 8개를
///    정답 위치 순서대로 연결 (왼→오, 앞→뒤, 마지막 칸=빈칸)
/// 4. PuzzleTrigger에서 이 스크립트를 Puzzle 슬롯에 연결
/// </summary>
public class Stage3_SlidingPuzzle : CameraPuzzleBase
{
	[Header("그리드 설정")]
	[Tooltip("3이면 3x3 그리드 (타일 8개 + 빈칸 1개)")]
	[SerializeField] private int gridSize = 3;

	[Tooltip("그리드의 왼쪽-앞 기준점 Transform. 타일 위치 계산의 기준이 됩니다.")]
	[SerializeField] private Transform gridOrigin;

	[Tooltip("타일 하나의 크기 (타일 간 간격). 타일 모델 크기에 맞게 조정하세요.")]
	[SerializeField] private float cellSize = 0.5f;

	[Tooltip("타일이 슬라이드될 때 걸리는 시간 (초). 0.1~0.2 정도가 자연스럽습니다.")]
	[SerializeField] private float slideDuration = 0.15f;

	[Header("타일 오브젝트 목록")]
	[Tooltip("씬에 배치된 SlidingTile 오브젝트들. (gridSize*gridSize - 1)개.\n" +
			 "정답 위치 순서대로 연결하세요: [0,0], [0,1], [0,2], [1,0] ... (마지막 빈칸 제외)")]
	[SerializeField] private List<SlidingTile> tiles = new List<SlidingTile>();

	[Header("퍼즐 완료 보상")]
	[Tooltip("완료 시 플레이어 인벤토리에 추가될 아이템 ID")]
	[SerializeField] private string rewardItemId = "doll_part_head";
	[SerializeField] private string rewardItemName = "인형 머리";
	[SerializeField] private string rewardDescription = "종이인형의 조각이다.";

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "조각을 얻었다.";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "아직 다 못 풀었다.";

	[Header("셔플 강도")]
	[Tooltip("퍼즐 시작 시 랜덤 이동 횟수. 클수록 더 많이 섞입니다.")]
	[SerializeField] private int shuffleMoves = 60;

	// ── 그리드 데이터 ──
	// _grid[row, col] = 해당 위치에 있는 타일 (null = 빈칸)
	private SlidingTile[,] _grid;
	private int _emptyRow, _emptyCol;       // 빈칸의 현재 그리드 좌표
	private bool _isSlidingAnimating = false;

	// ────────────────────────────────────────────
	// 초기화
	// ────────────────────────────────────────────

	protected override void Awake()
	{
		base.Awake();
		BuildGrid();
	}

	/// <summary>
	/// tiles 리스트 순서를 기반으로 그리드를 구성합니다.
	/// tiles[0] = [0,0]의 정답 타일, tiles[1] = [0,1]의 정답 타일 ... 순서입니다.
	/// 마지막 칸([gridSize-1, gridSize-1])은 빈칸이 됩니다.
	/// </summary>
	private void BuildGrid()
	{
		_grid = new SlidingTile[gridSize, gridSize];
		int tileIndex = 0;

		for (int r = 0; r < gridSize; r++)
		{
			for (int c = 0; c < gridSize; c++)
			{
				// 마지막 칸 = 빈칸
				if (r == gridSize - 1 && c == gridSize - 1)
				{
					_grid[r, c] = null;
					_emptyRow = r;
					_emptyCol = c;
					continue;
				}

				if (tileIndex >= tiles.Count) break;

				SlidingTile tile = tiles[tileIndex++];
				if (tile == null) continue;

				_grid[r, c] = tile;
				tile.Initialize(this, r, c);                // 정답 위치 주입
				tile.transform.position = GridToWorld(r, c); // 초기 위치 = 정답 위치
			}
		}
	}

	// ────────────────────────────────────────────
	// 퍼즐 시작 / 종료
	// ────────────────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();
		// 카메라 전환 완료 후 셔플 (약간 딜레이)
		StartCoroutine(ShuffleAfterDelay(0.3f));
	}

	private IEnumerator ShuffleAfterDelay(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		ShuffleTiles();
	}

	public override void ExitPuzzle()
	{
		GameServices.UI?.ShowDialogue(speaker, exitDialogue);
		base.ExitPuzzle();
		// 주의: 나가면 리셋하지 않습니다. 진행 상태가 유지됩니다.
		// 기획서에 "나가면 리셋"이 명시된 경우 여기서 RebuildAndShuffle() 호출 추가.
	}

	// ────────────────────────────────────────────
	// 셔플
	// ────────────────────────────────────────────

	private void ShuffleTiles()
	{
		// 유효한 랜덤 이동만 반복 → 항상 풀 수 있는 상태 보장
		int[] dr = { -1, 1, 0, 0 };
		int[] dc = { 0, 0, -1, 1 };
		int lastOpposite = -1;

		for (int i = 0; i < shuffleMoves; i++)
		{
			// 이동 가능한 방향 수집
			var validDirs = new List<int>();
			for (int d = 0; d < 4; d++)
			{
				if (d == lastOpposite) continue; // 방금 반대 방향으로 되돌아가기 방지
				int nr = _emptyRow + dr[d];
				int nc = _emptyCol + dc[d];
				if (nr >= 0 && nr < gridSize && nc >= 0 && nc < gridSize)
					validDirs.Add(d);
			}

			int chosen = validDirs[Random.Range(0, validDirs.Count)];
			int tileRow = _emptyRow + dr[chosen];
			int tileCol = _emptyCol + dc[chosen];

			// 즉시 스왑 (셔플 중엔 애니메이션 없음)
			SlidingTile movingTile = _grid[tileRow, tileCol];
			_grid[_emptyRow, _emptyCol] = movingTile;
			_grid[tileRow, tileCol] = null;
			movingTile.transform.position = GridToWorld(_emptyRow, _emptyCol);
			(_emptyRow, _emptyCol) = (tileRow, tileCol);

			// 반대 방향 기록 (다음 이동에서 되돌아가기 방지)
			lastOpposite = chosen < 2 ? (chosen == 0 ? 1 : 0) : (chosen == 2 ? 3 : 2);
		}

		Debug.Log($"[SlidingPuzzle] 셔플 완료 ({shuffleMoves}회)");
	}

	// ────────────────────────────────────────────
	// 타일 슬라이드 (SlidingTile.OnMouseDown → 호출됨)
	// ────────────────────────────────────────────

	/// <summary>
	/// SlidingTile이 클릭됐을 때 호출됩니다.
	/// 빈칸과 인접한 타일이면 슬라이드시킵니다.
	/// </summary>
	public void TrySlide(SlidingTile tile)
	{
		if (_isSlidingAnimating || isSolved) return;

		// 타일의 현재 그리드 좌표 찾기
		int tileRow = -1, tileCol = -1;
		for (int r = 0; r < gridSize; r++)
			for (int c = 0; c < gridSize; c++)
				if (_grid[r, c] == tile) { tileRow = r; tileCol = c; }

		if (tileRow < 0) return;

		// 빈칸과 상하좌우 1칸 인접인지 확인
		bool adjacent = (Mathf.Abs(tileRow - _emptyRow) == 1 && tileCol == _emptyCol) ||
						(Mathf.Abs(tileCol - _emptyCol) == 1 && tileRow == _emptyRow);

		if (!adjacent) return;

		StartCoroutine(SlideAnimation(tile, tileRow, tileCol));
	}

	private IEnumerator SlideAnimation(SlidingTile tile, int fromRow, int fromCol)
	{
		_isSlidingAnimating = true;

		Vector3 startPos = tile.transform.position;
		Vector3 endPos = GridToWorld(_emptyRow, _emptyCol);

		// 그리드 배열 먼저 업데이트
		_grid[_emptyRow, _emptyCol] = tile;
		_grid[fromRow, fromCol] = null;
		(_emptyRow, _emptyCol) = (fromRow, fromCol);

		// 부드럽게 이동
		float elapsed = 0f;
		while (elapsed < slideDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / slideDuration);
			// EaseOut 곡선으로 자연스럽게
			t = 1f - (1f - t) * (1f - t);
			tile.transform.position = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}
		tile.transform.position = endPos;

		_isSlidingAnimating = false;
		CheckSolution();
	}

	// ────────────────────────────────────────────
	// 정답 판정
	// ────────────────────────────────────────────

	protected override bool IsSolutionCorrect()
	{
		for (int r = 0; r < gridSize; r++)
		{
			for (int c = 0; c < gridSize; c++)
			{
				SlidingTile tile = _grid[r, c];
				if (tile == null) continue; // 빈칸은 무시
											// 타일이 자신의 정답 위치가 아니면 실패
				if (tile.targetRow != r || tile.targetCol != c) return false;
			}
		}
		return true;
	}

	protected override void SolvePuzzle()
	{
		// 인형 조각 아이템 지급
		var player = GameServices.Player;
		if (player != null)
		{
			var item = new ClueItem(rewardItemId, rewardItemName, rewardDescription);
			player.Inventory.AddItem(item);
			GameManager.Instance?.ClueTracker.RegisterClue(rewardItemId);
		}

		GameServices.UI?.ShowDialogue(speaker, solveDialogue);
		base.SolvePuzzle(); // isSolved = true, 카메라 원위치
	}

	// ────────────────────────────────────────────
	// 유틸리티
	// ────────────────────────────────────────────

	/// <summary>그리드 좌표(row, col)를 월드 위치로 변환합니다.</summary>
	private Vector3 GridToWorld(int row, int col)
	{
		if (gridOrigin == null) return Vector3.zero;
		// right 방향으로 열(col), forward 방향으로 행(row)
		return gridOrigin.position
			 + gridOrigin.right * (col * cellSize)
			 + gridOrigin.forward * (row * cellSize);
	}
}