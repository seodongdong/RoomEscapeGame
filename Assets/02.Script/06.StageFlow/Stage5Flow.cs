using UnityEngine;

/// <summary>
/// 5스테이지(지하실) 진행 흐름 추적.
///
/// 설계 흐름:
/// 입장 → 목각인형장식(0~4) → 퍼즐완료 → 열쇠획득 → 상자개방
///   → 추격전시작 → 소녀구출/캠코더수집(병렬) → 대문도달 → 엔딩분기
///
/// [연결 방법 - Inspector]
/// basementPuzzle, girlRescueTrigger, chaseSequence 슬롯에
/// 씬의 해당 오브젝트를 연결하세요.
/// </summary>
public class Stage5Flow : StageFlowBase
{
	[Header("연결 — 기존 오브젝트 그대로 참조")]
	[SerializeField] private Stage5_BasementPuzzle basementPuzzle;
	[SerializeField] private ChaseSequence chaseSequence;

	protected override void Awake()
	{
		base.Awake();

		if (basementPuzzle != null)
			basementPuzzle.OnPuzzleSolved += () => SetStep("KeyAcquired");
	}

	private void Update()
	{
		if (currentStep == "Entering")
			SetStep("DollsPlacing");

		if (currentStep == "KeyAcquired")
			SetStep("BoxOpened");

		if (chaseSequence != null && chaseSequence.IsGirlRescued && currentStep != "GirlRescued" && currentStep != "ExitReached" && currentStep != "EndingTriggered")
			SetStep("GirlRescued");
	}
}
