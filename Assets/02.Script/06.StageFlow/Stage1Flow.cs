using UnityEngine;

/// <summary>
/// 1스테이지(거실) 진행 흐름 추적.
///
/// 설계 흐름:
/// 입장 → TV강제관람유도 → TV4회시청 → 크리처등장 → 퍼즐해금 → 퍼즐해결 → 탈출 → 완료
///
/// [연결 방법 - Inspector]
/// 1. 빈 GameObject(예: "Stage1Flow")를 씬에 배치
/// 2. 이 스크립트 부착
/// 3. tvPlayer, dollHousePuzzle, exitDoor 슬롯에 씬의 해당 오브젝트 연결
///
/// [기존 로직 변경 없음]
/// TVPlayer, Stage1_DollHousePuzzle, PuzzleSolveDoor의 내부 동작은
/// 전혀 건드리지 않았습니다. 이 클래스는 그들의 이벤트를 구독해
/// currentStep 텍스트만 갱신합니다.
/// </summary>
public class Stage1Flow : StageFlowBase
{
	[Header("연결 — 기존 오브젝트 그대로 참조")]
	[SerializeField] private TVPlayer tvPlayer;
	[SerializeField] private Stage1_DollHousePuzzle dollHousePuzzle;
	[SerializeField] private PuzzleSolveDoor exitDoor;

	protected override void Awake()
	{
		base.Awake();

		if (dollHousePuzzle != null)
			dollHousePuzzle.OnPuzzleSolved += () => SetStep("PuzzleSolved");
	}

	private void Update()
	{
		// TVPlayer는 viewCount를 private으로 들고 있어 이벤트가 없으므로,
		// 가장 단순한 방식으로 폴링합니다. (TVPlayer 자체를 건드리지 않기 위한 선택)
		// 추후 TVPlayer에 OnViewCountChanged 이벤트를 추가하면 폴링을 제거할 수 있습니다.
		if (tvPlayer == null) return;

		if (currentStep == "Entering")
			SetStep("WaitingForTV");

		// CanInteract가 false가 되는 시점(4회 시청 완료) 감지
		if (currentStep == "WaitingForTV" && !tvPlayer.CanInteract(GameServices.Player))
			SetStep("CreatureRevealed");

		if (currentStep == "CreatureRevealed" && dollHousePuzzle != null && !dollHousePuzzle.IsSolved)
			SetStep("PuzzleUnlocked");
	}
}
