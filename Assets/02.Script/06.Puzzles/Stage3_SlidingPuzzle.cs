using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 3스테이지: 슬라이딩 퍼즐 (미로)
/// 기획서: 곰돌이 → 옷 → 밧줄/청테이프 → 상자 (4개)
/// 난이도 점진적 증가
/// </summary>
public class Stage3_SlidingPuzzle : PuzzleBase
{
	[System.Serializable]
	public class PuzzleData
	{
		public Sprite targetImage;      // 맞춰야 할 이미지
		public int gridSize;            // 3x3, 4x4 등
		public string bodyPartReward;   // "head", "body", "arm_left" 등
	}

	[SerializeField] private GameObject dollCreature; 

	[Header("Puzzle Sequence")]
	[SerializeField] private List<PuzzleData> puzzleSequence; // 4개

	[Header("UI")]
	[SerializeField] private Image[] tileImages;
	[SerializeField] private Button[] tileButtons;

	private int _currentPuzzleIndex = 0;
	private List<int> _tilePositions;
	private int _emptyTileIndex;
	private List<string> _collectedBodyParts = new List<string>();

	public override void StartPuzzle()
	{
		base.StartPuzzle();
		LoadPuzzle(_currentPuzzleIndex);
	}

	private void LoadPuzzle(int index)
	{
		if (index >= puzzleSequence.Count) return;

		var puzzleData = puzzleSequence[index];

		// 타일 초기화
		InitializeTiles(puzzleData.gridSize);

		Debug.Log($"[SlidingPuzzle] {index + 1}번째 퍼즐 시작: {puzzleData.bodyPartReward}");
	}

	private void InitializeTiles(int gridSize)
	{
		int totalTiles = gridSize * gridSize;
		_tilePositions = new List<int>();

		for (int i = 0; i < totalTiles; i++)
		{
			_tilePositions.Add(i);
		}

		// 마지막 타일은 빈 칸
		_emptyTileIndex = totalTiles - 1;

		// 섞기
		ShuffleTiles();
	}

	private void ShuffleTiles()
	{
		// TODO: 섞기 로직
	}

	public void MoveTile(int tileIndex)
	{
		// TODO: 타일 이동 로직

		if (IsPuzzleSolved())
		{
			HandlePuzzleSolved();
		}
	}

	private bool IsPuzzleSolved()
	{
		for (int i = 0; i < _tilePositions.Count; i++)
		{
			if (_tilePositions[i] != i) return false;
		}
		return true;
	}

	private void HandlePuzzleSolved()
	{
		var currentPuzzle = puzzleSequence[_currentPuzzleIndex];
		_collectedBodyParts.Add(currentPuzzle.bodyPartReward);

		Debug.Log($"[SlidingPuzzle] {currentPuzzle.bodyPartReward} 획득!");

		_currentPuzzleIndex++;

		if (_currentPuzzleIndex < puzzleSequence.Count)
		{
			LoadPuzzle(_currentPuzzleIndex);
		}
		else
		{
			// 모든 퍼즐 완료
			CompletePuzzle();
		}
	}

	private void CompletePuzzle()
	{
		Debug.Log("[SlidingPuzzle] 모든 조각 획득! 제작대로 이동 가능");
		if (dollCreature != null)
		{
			dollCreature.SetActive(false);
		}
		SolvePuzzle();
	}

	protected override bool IsSolutionCorrect()
	{
		return _collectedBodyParts.Count == puzzleSequence.Count;
	}
}