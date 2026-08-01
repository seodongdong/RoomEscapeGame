using UnityEngine;

/// <summary>
/// 3스테이지(미로) 진행 흐름 추적.
///
/// 설계 흐름:
/// 입장 → 손전등획득 → 슬라이딩퍼즐(그림 1~4) → 인형조각획득
///   → 인형조립 → 크리처에게 인계 → 출구개방 → 완료
///
/// [이번 수정]
/// 기존 주석에 "다단계 확장은 별도 작업 범위"라고 적어둔 부분을
/// Stage3_SlidingPuzzle이 실제로 지원하게 되어, 그림 진행도를
/// CurrentStep에 표시하도록 바꿨습니다.
/// 조립대(Stage3_DollAssemblyTable)와 인계(Stage3_DollHandover) 단계도 추가했습니다.
///
/// [연결 방법 - Inspector]
/// flashlightPickup / slidingPuzzle / assemblyTable / handover 슬롯에
/// 씬의 해당 오브젝트를 연결하세요.
/// </summary>
public class Stage3Flow : StageFlowBase
{
	[Header("연결 — 기존 오브젝트 그대로 참조")]
	[SerializeField] private FlashlightPickup flashlightPickup;
	[SerializeField] private Stage3_SlidingPuzzle slidingPuzzle;
	[SerializeField] private Stage3_DollAssemblyTable assemblyTable;
	[SerializeField] private Stage3_DollHandover handover;

	private int _lastPictureIndex = -1;

	protected override void Awake()
	{
		base.Awake();

		if (slidingPuzzle != null)
			slidingPuzzle.OnPuzzleSolved += () => SetStep("DollPartsCollected");

		if (assemblyTable != null)
			assemblyTable.OnPuzzleSolved += () => SetStep("DollAssembled");
	}

	private void Update()
	{
		if (currentStep == "Entering")
			SetStep("FlashlightAcquired");

		// 슬라이딩 퍼즐 그림 진행도
		if (slidingPuzzle != null && !slidingPuzzle.IsSolved)
		{
			if (slidingPuzzle.CurrentPictureIndex != _lastPictureIndex)
			{
				_lastPictureIndex = slidingPuzzle.CurrentPictureIndex;
				SetStep($"SlidingPicture_{_lastPictureIndex + 1}of{slidingPuzzle.TotalPictureCount}");
			}
		}

		// 조립 완료 후 크리처 인계 대기
		if (currentStep == "DollAssembled")
			SetStep("WaitingHandover");

		// 인계 완료 → 출구 개방
		if (currentStep == "WaitingHandover" && handover != null && handover.HasHandedOver)
			SetStep("ExitUnlocked");
	}
}