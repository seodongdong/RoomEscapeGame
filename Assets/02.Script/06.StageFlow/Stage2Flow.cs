using UnityEngine;

/// <summary>
/// 2스테이지(장례식장) 진행 흐름 추적.
///
/// 설계 흐름:
/// 입장 → 관찰유도 → 퍼즐시작(관 클릭) → 3단계기억퍼즐 → 퍼즐완료
///   → 작은방진입 → 점프스케어(조건부, 1회) → 퇴장연출 → 완료
///
/// [연결 방법 - Inspector]
/// candyPuzzle, shadowCreature, jumpscareTrigger, exitTrigger 슬롯에
/// 씬의 해당 오브젝트를 연결하세요.
///
/// [기존 로직 변경 없음]
/// Stage2_AltarCandyPuzzle, Stage2_ShadowCreature, Stage2_JumpscareTrigger,
/// Stage2_ExitTrigger의 내부 동작은 전혀 건드리지 않았습니다.
/// </summary>
public class Stage2Flow : StageFlowBase
{
	[Header("연결 — 기존 오브젝트 그대로 참조")]
	[SerializeField] private Stage2_AltarCandyPuzzle candyPuzzle;
	[SerializeField] private Stage2_ShadowCreature shadowCreature;

	protected override void Awake()
	{
		base.Awake();

		if (candyPuzzle != null)
			candyPuzzle.OnPuzzleSolved += () => SetStep("PuzzleSolved");
	}

	private void Update()
	{
		if (currentStep == "Entering")
			SetStep("PuzzleEntered");

		// 퍼즐 해결 후 ShadowCreature가 작은 방 앞에서 이동했는지는
		// 직접 이벤트가 없으므로, 퍼즐 완료 시점을 기준으로 단계만 진행합니다.
		if (currentStep == "PuzzleSolved")
			SetStep("SmallRoomUnlocked");
	}
}
