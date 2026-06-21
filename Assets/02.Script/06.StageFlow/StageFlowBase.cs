using UnityEngine;

/// <summary>
/// 스테이지 진행 흐름의 공통 베이스.
///
/// [목적]
/// 각 스테이지의 단계(예: Stage1 - 입장 → TV시청 → 크리처등장 →
/// 퍼즐해금 → 퍼즐해결 → 탈출)는 현재 여러 스크립트의 이벤트 구독으로
/// 암묵적으로 연결되어 있습니다(TVPlayer가 크리처를 활성화하고,
/// 퍼즐의 OnPuzzleSolved를 Door가 구독하는 식).
///
/// 이 자체는 나쁜 패턴이 아니지만, "지금 이 스테이지가 어느 단계인지"를
/// 한눈에 보여주는 곳이 코드에 없었습니다. StageFlow는 그 진행 단계를
/// "표시"하는 역할만 합니다 — 기존 스크립트들의 이벤트 연결을 대체하지
/// 않고, 그 위에 옵저버로 얹혀서 현재 단계를 추적합니다.
///
/// [중요: 기존 로직을 변경하지 않습니다]
/// StageFlow는 퍼즐을 풀거나 문을 여는 로직을 직접 수행하지 않습니다.
/// 각 스테이지의 실제 게임플레이 스크립트(TVPlayer, Stage1_DollHousePuzzle 등)는
/// 그대로 유지하고, 그 스크립트들이 발행하는 이벤트를 구독해
/// CurrentStep만 갱신합니다. 디버깅 시 "Inspector에서 CurrentStep만
/// 보면 지금 막힌 지점을 알 수 있다"는 게 핵심 가치입니다.
///
/// [씬 배치]
/// 각 스테이지 씬에 빈 GameObject로 하나 배치하고,
/// Stage1Flow처럼 이를 상속한 구체 클래스를 붙입니다.
/// </summary>
public abstract class StageFlowBase : MonoBehaviour
{
	[Header("디버그용 — 현재 진행 단계 (읽기 전용)")]
	[SerializeField] protected string currentStep = "Entering";

	public string CurrentStep => currentStep;

	public event System.Action<string> OnStepChanged;

	protected virtual void SetStep(string step)
	{
		if (currentStep == step) return;
		currentStep = step;
		OnStepChanged?.Invoke(step);
		Debug.Log($"[{GetType().Name}] 단계 변경 → {step}");
	}

	protected virtual void Awake()
	{
		SetStep("Entering");
	}
}
