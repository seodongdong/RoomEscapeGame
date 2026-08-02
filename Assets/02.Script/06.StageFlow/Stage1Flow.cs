using UnityEngine;

/// <summary>
/// 1스테이지(거실) 진행 흐름 추적.
///
/// 설계 흐름:
/// 입장 → TV강제관람유도 → TV4회시청 → 크리처등장 → 단서수집(0~N)
///   → 퍼즐해결 → 탈출 → 완료
///
/// [이번 수정]
/// Stage1_DollHousePickupClue로 거실 프랍을 몇 개나 주웠는지
/// CurrentStep에 표시하도록 했습니다. "퍼즐이 안 풀린다"는 문제가
/// 생겼을 때 단서를 덜 주운 건지 배치를 틀린 건지 바로 구분됩니다.
///
/// [연결 방법 - Inspector]
/// 1. 빈 GameObject(예: "Stage1Flow")를 씬에 배치
/// 2. 이 스크립트 부착
/// 3. tvPlayer, dollHousePuzzle, exitDoor 슬롯에 씬의 해당 오브젝트 연결
///
/// [기존 로직 변경 없음]
/// TVPlayer, Stage1_DollHousePuzzle, PuzzleSolveDoor의 내부 동작은
/// 전혀 건드리지 않았습니다. 이 클래스는 그들의 상태를 읽어
/// currentStep 텍스트만 갱신합니다.
/// </summary>
public class Stage1Flow : StageFlowBase
{
	[Header("연결 — 기존 오브젝트 그대로 참조")]
	[SerializeField] private TVPlayer tvPlayer;
	[SerializeField] private Stage1_DollHousePuzzle dollHousePuzzle;
	[SerializeField] private PuzzleSolveDoor exitDoor;

	private int _lastCollectedCount = -1;

	protected override void Awake()
	{
		base.Awake();

		if (dollHousePuzzle != null)
			dollHousePuzzle.OnPuzzleSolved += () => SetStep("PuzzleSolved");
	}

	private void Update()
	{
		if (tvPlayer == null) return;

		if (currentStep == "Entering")
			SetStep("WaitingForTV");

		// TV 4회 시청 완료 감지 (CanInteract가 false로 바뀌는 시점)
		if (currentStep == "WaitingForTV" && !tvPlayer.CanInteract(GameServices.Player))
			SetStep("CreatureRevealed");

		// 크리처 등장 이후 — 단서 수집 진행도 표시
		if (dollHousePuzzle != null && !dollHousePuzzle.IsSolved &&
			currentStep != "WaitingForTV" && currentStep != "Entering")
		{
			int collected = dollHousePuzzle.CollectedItemCount;
			if (collected != _lastCollectedCount)
			{
				_lastCollectedCount = collected;
				SetStep($"Collecting_{collected}of{dollHousePuzzle.RequiredItemCount}");
			}
		}
	}
}