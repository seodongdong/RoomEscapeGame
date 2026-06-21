using UnityEngine;

/// <summary>
/// 3스테이지(미로) 진행 흐름 추적.
///
/// 설계 흐름:
/// 입장 → 손전등획득 → 인형조각수집(0~4) → 슬라이딩퍼즐 → 인형조립
///   → 출구개방 → 완료
///
/// [참고]
/// 기획서상 슬라이딩 퍼즐은 그림 4개(곰돌이→인형옷→밧줄/청테이프→상자)를
/// 순차적으로 완성하는 구조이지만, 현재 Stage3_SlidingPuzzle 구현은
/// 3x3 그리드 1개만 완성하면 종료되는 단순화된 버전입니다. 이 Flow는
/// 현재 구현 기준으로 단계를 추적하며, 다단계 확장은 별도 작업 범위입니다.
///
/// [연결 방법 - Inspector]
/// flashlightPickup, slidingPuzzle 슬롯에 씬의 해당 오브젝트를 연결하세요.
/// </summary>
public class Stage3Flow : StageFlowBase
{
	[Header("연결 — 기존 오브젝트 그대로 참조")]
	[SerializeField] private FlashlightPickup flashlightPickup;
	[SerializeField] private Stage3_SlidingPuzzle slidingPuzzle;

	protected override void Awake()
	{
		base.Awake();

		if (slidingPuzzle != null)
			slidingPuzzle.OnPuzzleSolved += () => SetStep("DollAssembled");
	}

	private void Update()
	{
		if (currentStep == "Entering")
			SetStep("FlashlightAcquired");

		if (currentStep == "FlashlightAcquired" && slidingPuzzle != null && !slidingPuzzle.IsSolved)
			SetStep("SlidingPuzzleEntered");

		if (currentStep == "DollAssembled")
			SetStep("ExitUnlocked");
	}
}
