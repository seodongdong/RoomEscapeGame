using UnityEngine;

/// <summary>
/// 모든 상호작용 가능한 오브젝트의 공통 베이스.
///
/// [기존 문제]
/// Stage1TVGate(구 Stage1TVPriorityManager).CheckPriorityBlocked(player) 한 줄이
/// DiaryClue, UsableItemClue, RecordingDoll, ObjectViewer3D,
/// Stage1_DoorWithPriorityCheck, PuzzleTrigger 6곳에 복붙되어 있었고,
/// Door, Camcorder, TVPlayer 등에는 빠져 있었습니다.
/// 새 인터랙터블을 추가할 때 이 한 줄을 잊으면 "TV를 안 봐도
/// 다른 행동이 가능한" 조용한 버그가 생기는 구조였습니다.
///
/// [해결 방식]
/// IInteractable.Interact()를 직접 구현하지 않고, 이 베이스 클래스를
/// 상속해 OnInteract()만 오버라이드하면 우선순위 체크가 항상
/// 자동으로 먼저 실행됩니다. 새 스크립트를 추가할 때 깜빡할 수 있는
/// 여지를 구조적으로 없앤 것입니다.
///
/// [기존 스크립트 전환 방법]
/// 기존:
///   public class DiaryClue : MonoBehaviour, IInteractable
///   {
///       public void Interact(IPlayer player)
///       {
///           if (Stage1TVGate.CheckPriorityBlocked(player)) return;
///           // ... 실제 로직
///       }
///   }
///
/// 변경 후:
///   public class DiaryClue : InteractableBase
///   {
///       protected override void OnInteract(IPlayer player)
///       {
///           // ... 실제 로직 (우선순위 체크는 베이스가 이미 처리함)
///       }
///   }
///
/// CanInteract()와 InteractionPrompt는 그대로 각 하위 클래스가
/// 구현해야 합니다(오브젝트마다 의미가 다르기 때문에 공통화하지 않음).
///
/// [Stage1 외 씬에서의 동작]
/// Stage1TVGate.Instance가 null인 씬(Stage2~5)에서는
/// CheckPriorityBlocked가 항상 false를 반환하므로 정상 동작합니다.
/// 기존 동작과 완전히 동일합니다 — 이 베이스 클래스는 호출 위치만
/// 강제할 뿐, 차단 로직 자체는 바꾸지 않았습니다.
/// </summary>
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
	public abstract string InteractionPrompt { get; }

	public abstract bool CanInteract(IPlayer player);

	/// <summary>
	/// IInteractable.Interact의 실제 구현 — 하위 클래스가 오버라이드할 수
	/// 없는 일반(virtual이 아닌) 메서드입니다. 우선순위 체크를 건너뛰는
	/// 길을 구조적으로 막기 위함입니다.
	/// </summary>
	public void Interact(IPlayer player)
	{
		if (Stage1TVGate.CheckPriorityBlocked(player))
			return;

		OnInteract(player);
	}

	/// <summary>
	/// 하위 클래스가 구현할 실제 상호작용 로직.
	/// 우선순위 체크는 이미 통과한 상태에서 호출됩니다.
	/// </summary>
	protected abstract void OnInteract(IPlayer player);
}
