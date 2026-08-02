using UnityEngine;

/// <summary>
/// 2스테이지(장례식장) 진행 흐름 추적.
///
/// 설계 흐름:
/// 입장 → 불빛관찰 → 1단계클리어 → 2단계클리어 → 3단계클리어(퍼즐완료)
///   → 작은방개방 → 퇴장연출 → 완료
///
/// [이번 수정]
/// 퍼즐이 Stage2_AltarCandyPuzzle(사탕 배치)에서
/// Stage2_LightSequencePuzzle(불빛 순서 기억)로 교체됨에 따라 참조를 변경하고,
/// 3단계 진행 상황을 CurrentStep에 그대로 보여주도록 했습니다.
/// Inspector에서 lightPuzzle 슬롯만 다시 연결하면 됩니다.
///
/// [기존 로직 변경 없음]
/// 이 클래스는 여전히 "표시"만 합니다. 퍼즐을 풀거나 문을 여는 로직은
/// 각 게임플레이 스크립트가 그대로 담당합니다.
/// </summary>
public class Stage2Flow : StageFlowBase
{
	[Header("연결 — 기존 오브젝트 그대로 참조")]
	[SerializeField] private Stage2_LightSequencePuzzle lightPuzzle;
	[SerializeField] private Stage2_ShadowCreature shadowCreature;

	private int _lastReportedStage = -1;

	protected override void Awake()
	{
		base.Awake();

		if (lightPuzzle != null)
			lightPuzzle.OnPuzzleSolved += () => SetStep("PuzzleSolved");
	}

	private void Update()
	{
		if (currentStep == "Entering")
			SetStep("WatchingLights");

		if (lightPuzzle == null) return;

		// 진행 중인 단계 표시 (1/3 → 2/3 → 3/3)
		if (!lightPuzzle.IsSolved && lightPuzzle.CurrentStageIndex != _lastReportedStage)
		{
			_lastReportedStage = lightPuzzle.CurrentStageIndex;
			SetStep($"MemoryStage_{_lastReportedStage + 1}of{lightPuzzle.TotalStageCount}");
		}

		if (currentStep == "PuzzleSolved")
			SetStep("SmallRoomUnlocked");
	}
}