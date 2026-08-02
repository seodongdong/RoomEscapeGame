using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 3스테이지 슬라이딩 퍼즐 — 그림 순차 진행 버전
///
/// [기획서]
/// - 슬라이딩 퍼즐 방식으로 4개의 그림을 순서대로 완성
///   1번 곰돌이 → 2번 인형옷 → 3번 밧줄과 청테이프 → 4번 상자
/// - 조각을 마우스로 상하좌우로 밀어서 맞춤 (2D UI 아님, 3D 월드에서 직접 조작)
/// - 난이도는 순서가 뒤로 갈수록 어려워짐
/// - 한 그림 완성 → 다음 그림이 섞인 채로 자동 전환
/// - 4개 모두 완성 시 퍼즐에서 나가짐 + 인형 몸조각 획득
/// - 풀다가 나가면 퍼즐 리셋. 처음부터 다시 해야 함
///
/// [이전 버전과의 차이]
/// 기존 구현은 3x3 그리드 1개만 맞추면 끝나는 단순화 버전이었습니다.
/// 그리드 로직(셔플/슬라이드/판정)은 그대로 재사용하고, 그 위에
/// "그림 목록"과 "완성 시 다음 그림으로 전환"만 얹었습니다.
///
/// [미로에 4개 스테이션을 두고 싶다면]
/// pictures에 그림을 1개만 넣은 이 컴포넌트를 코너마다 배치하고,
/// rewardItems에 그 코너에서 주는 인형 조각을 넣으면 됩니다.
/// 한 곳에서 4개를 전부 풀게 하려면 pictures에 4개를 넣으세요.
///
/// [씬 설정]
/// 1. 빈 오브젝트에 이 스크립트 부착
/// 2. gridOrigin: 그리드의 왼쪽-앞 기준점
/// 3. tiles: SlidingTile 8개 (3x3 기준)를 정답 위치 순서대로 연결
///    → 왼→오, 앞→뒤 순서. 마지막 칸이 빈칸입니다.
/// 4. pictures: 그림 텍스처 4개 (곰돌이 / 인형옷 / 밧줄과 청테이프 / 상자)
/// 5. PuzzleTrigger의 Puzzle 슬롯에 이 스크립트 연결
/// </summary>
public class Stage3_SlidingPuzzle : CameraPuzzleBase
{
	[System.Serializable]
	public class PictureStep
	{
		[Tooltip("디버그/대사용 이름. 예: 곰돌이")]
		public string displayName = "곰돌이";
		[Tooltip("이 단계에서 맞출 그림 텍스처")]
		public Texture picture;
		[Tooltip("셔플 횟수. 뒤 그림일수록 크게 하면 난이도가 올라갑니다.")]
		public int shuffleMoves = 40;
		[TextArea(1, 3)]
		public string clearDialogue = "...하나 맞췄다.";
	}

	[System.Serializable]
	public class RewardItem
	{
		public string itemId = "doll_part_head";
		public string itemName = "인형 머리";
		[TextArea(1, 2)]
		public string description = "종이인형의 조각이다.";
		public GameObject itemPrefab;
	}

	[Header("그림 목록 (순서대로 진행)")]
	[SerializeField]
	private List<PictureStep> pictures = new List<PictureStep>()
	{
		new PictureStep { displayName = "곰돌이",           shuffleMoves = 30, clearDialogue = "곰인형 그림이다..." },
		new PictureStep { displayName = "인형옷",           shuffleMoves = 50, clearDialogue = "옷 그림이네." },
		new PictureStep { displayName = "밧줄과 청테이프",  shuffleMoves = 80, clearDialogue = "...밧줄이랑 테이프?" },
		new PictureStep { displayName = "상자",             shuffleMoves = 120, clearDialogue = "...상자다. 커다란 상자." },
	};

	[Header("그리드 설정")]
	[SerializeField] private int gridSize = 3;
	[SerializeField] private Transform gridOrigin;
	[SerializeField] private float cellSize = 0.5f;
	[SerializeField] private float slideDuration = 0.15f;

	[Header("타일 오브젝트 목록")]
	[Tooltip("(gridSize*gridSize - 1)개. 정답 위치 순서대로: [0,0], [0,1], [0,2], [1,0] ...")]
	[SerializeField] private List<SlidingTile> tiles = new List<SlidingTile>();

	[Header("퍼즐 완료 보상 (인형 조각)")]
	[Tooltip("모든 그림을 완성했을 때 지급할 인형 조각들")]
	[SerializeField]
	private List<RewardItem> rewardItems = new List<RewardItem>()
	{
		new RewardItem { itemId = "doll_part_head",      itemName = "인형 머리" },
		new RewardItem { itemId = "doll_part_arm_left",  itemName = "인형 왼팔" },
		new RewardItem { itemId = "doll_part_arm_right", itemName = "인형 오른팔" },
		new RewardItem { itemId = "doll_part_leg_left",  itemName = "인형 왼다리" },
		new RewardItem { itemId = "doll_part_leg_right", itemName = "인형 오른다리" },
	};

	[Header("크리처 연동 (그림 완성마다 기괴해짐)")]
	[SerializeField] private Stage3_DollCreature dollCreature;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string startDialogue = "그림을 맞춰봐야겠다.";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "...인형 조각을 전부 모았다.";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "아직 다 못 풀었다. 처음부터 다시 해야겠네...";

	// ── 그리드 데이터 ────────────────────────────────────────
	private SlidingTile[,] _grid;
	private int _emptyRow, _emptyCol;
	private bool _isSlidingAnimating = false;
	private int _pictureIndex = 0;

	public int CurrentPictureIndex => _pictureIndex;
	public int TotalPictureCount => pictures.Count;

	// ── 초기화 ────────────────────────────────────────────────

	protected override void Awake()
	{
		base.Awake();
		BuildGrid();
		ApplyPicture(0);
	}

	private void BuildGrid()
	{
		_grid = new SlidingTile[gridSize, gridSize];
		int tileIndex = 0;

		for (int r = 0; r < gridSize; r++)
		{
			for (int c = 0; c < gridSize; c++)
			{
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
				tile.Initialize(this, r, c);
				tile.transform.position = GridToWorld(r, c);
			}
		}
	}

	/// <summary>그리드를 정답 상태로 되돌립니다 (셔플 전 기준 상태).</summary>
	private void RestoreSolvedLayout()
	{
		int tileIndex = 0;
		for (int r = 0; r < gridSize; r++)
		{
			for (int c = 0; c < gridSize; c++)
			{
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
				tile.targetRow = r;
				tile.targetCol = c;
				tile.transform.position = GridToWorld(r, c);
			}
		}
	}

	private void ApplyPicture(int index)
	{
		if (pictures == null || index >= pictures.Count) return;

		Texture tex = pictures[index].picture;
		foreach (var tile in tiles)
			tile?.ApplyPicture(tex, gridSize);
	}

	// ── 퍼즐 시작 / 종료 ─────────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		GameServices.UI?.ShowDialogue(speaker, startDialogue);
		StartCoroutine(ShuffleAfterDelay(0.3f));
	}

	private IEnumerator ShuffleAfterDelay(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		ShuffleTiles(CurrentShuffleMoves());
		SetTilesInteractable(true);
	}

	private int CurrentShuffleMoves()
	{
		if (pictures == null || _pictureIndex >= pictures.Count) return 40;
		return Mathf.Max(4, pictures[_pictureIndex].shuffleMoves);
	}

	/// <summary>ESC / 향로 등으로 나갈 때 — 기획서: 퍼즐 전체 리셋</summary>
	public override void ExitPuzzle()
	{
		SetTilesInteractable(false);

		if (!isSolved)
		{
			GameServices.UI?.ShowDialogue(speaker, exitDialogue);
			ResetPuzzleCompletely();
		}

		base.ExitPuzzle();
	}

	private void ResetPuzzleCompletely()
	{
		_pictureIndex = 0;
		RestoreSolvedLayout();
		ApplyPicture(0);
		Debug.Log("[SlidingPuzzle] 퍼즐 전체 리셋 (1번 그림부터)");
	}

	private void SetTilesInteractable(bool interactable)
	{
		foreach (var tile in tiles)
			tile?.SetInteractable(interactable);
	}

	// ── 셔플 ─────────────────────────────────────────────────

	private void ShuffleTiles(int moves)
	{
		int[] dr = { -1, 1, 0, 0 };
		int[] dc = { 0, 0, -1, 1 };
		int lastOpposite = -1;

		for (int i = 0; i < moves; i++)
		{
			var validDirs = new List<int>();
			for (int d = 0; d < 4; d++)
			{
				if (d == lastOpposite) continue;
				int nr = _emptyRow + dr[d];
				int nc = _emptyCol + dc[d];
				if (nr >= 0 && nr < gridSize && nc >= 0 && nc < gridSize)
					validDirs.Add(d);
			}
			if (validDirs.Count == 0) break;

			int chosen = validDirs[Random.Range(0, validDirs.Count)];
			int tileRow = _emptyRow + dr[chosen];
			int tileCol = _emptyCol + dc[chosen];

			SlidingTile movingTile = _grid[tileRow, tileCol];
			_grid[_emptyRow, _emptyCol] = movingTile;
			_grid[tileRow, tileCol] = null;
			if (movingTile != null)
				movingTile.transform.position = GridToWorld(_emptyRow, _emptyCol);
			(_emptyRow, _emptyCol) = (tileRow, tileCol);

			lastOpposite = chosen < 2 ? (chosen == 0 ? 1 : 0) : (chosen == 2 ? 3 : 2);
		}

		Debug.Log($"[SlidingPuzzle] {_pictureIndex + 1}번 그림 셔플 완료 ({moves}회)");
	}

	// ── 타일 슬라이드 ────────────────────────────────────────

	public void TrySlide(SlidingTile tile)
	{
		if (_isSlidingAnimating || isSolved) return;

		int tileRow = -1, tileCol = -1;
		for (int r = 0; r < gridSize; r++)
			for (int c = 0; c < gridSize; c++)
				if (_grid[r, c] == tile) { tileRow = r; tileCol = c; }

		if (tileRow < 0) return;

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

		_grid[_emptyRow, _emptyCol] = tile;
		_grid[fromRow, fromCol] = null;
		(_emptyRow, _emptyCol) = (fromRow, fromCol);

		float elapsed = 0f;
		while (elapsed < slideDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / slideDuration);
			t = 1f - (1f - t) * (1f - t);
			tile.transform.position = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}
		tile.transform.position = endPos;

		_isSlidingAnimating = false;

		if (IsSolutionCorrect())
			StartCoroutine(OnPictureCompleted());
	}

	// ── 그림 완성 → 다음 그림 ────────────────────────────────

	private IEnumerator OnPictureCompleted()
	{
		SetTilesInteractable(false);

		var step = pictures[_pictureIndex];
		GameServices.Audio?.PlaySFX("puzzle_correct");
		if (!string.IsNullOrEmpty(step.clearDialogue))
			GameServices.UI?.ShowDialogue(speaker, step.clearDialogue);

		// 크리처가 그림 하나 완성할 때마다 점점 기괴해짐
		if (dollCreature != null)
			dollCreature.NextState();

		yield return new WaitForSecondsRealtime(1.4f);

		_pictureIndex++;

		if (_pictureIndex >= pictures.Count)
		{
			SolvePuzzle();
			yield break;
		}

		// 다음 그림이 섞인 채로 자동 전환 (기획서)
		RestoreSolvedLayout();
		ApplyPicture(_pictureIndex);
		ShuffleTiles(CurrentShuffleMoves());
		SetTilesInteractable(true);

		Debug.Log($"[SlidingPuzzle] 다음 그림으로 전환 → {pictures[_pictureIndex].displayName}");
	}

	// ── 정답 판정 ────────────────────────────────────────────

	protected override bool IsSolutionCorrect()
	{
		for (int r = 0; r < gridSize; r++)
		{
			for (int c = 0; c < gridSize; c++)
			{
				SlidingTile tile = _grid[r, c];
				if (tile == null) continue;
				if (tile.targetRow != r || tile.targetCol != c) return false;
			}
		}
		return true;
	}

	// ── 퍼즐 완료 ────────────────────────────────────────────

	protected override void SolvePuzzle()
	{
		SetTilesInteractable(false);
		GrantRewards();

		GameServices.UI?.ShowDialogue(speaker, solveDialogue);
		GameServices.Audio?.PlaySFX("puzzle_solved");

		base.SolvePuzzle();
	}

	private void GrantRewards()
	{
		var player = GameServices.Player;
		if (player == null) return;

		foreach (var reward in rewardItems)
		{
			if (reward == null || string.IsNullOrEmpty(reward.itemId)) continue;
			ClueRegistrar.RegisterUsableItem(
				player, reward.itemId, reward.itemName, "", reward.description, reward.itemPrefab);
		}

		Debug.Log($"[SlidingPuzzle] 인형 조각 {rewardItems.Count}개 지급");
	}

	protected override void OnLoadStateSolved()
	{
		SetTilesInteractable(false);
		_pictureIndex = pictures.Count;
	}

	// ── 유틸리티 ─────────────────────────────────────────────

	private Vector3 GridToWorld(int row, int col)
	{
		if (gridOrigin == null) return Vector3.zero;
		return gridOrigin.position
			 + gridOrigin.right * (col * cellSize)
			 + gridOrigin.forward * (row * cellSize);
	}
}