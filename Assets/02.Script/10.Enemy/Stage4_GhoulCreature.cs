using UnityEngine;

/// <summary>
/// 4스테이지: 아귀
/// 기획서: "양반다리 테이블에 아귀앉아있고, 가만히 응시"
/// "음식 가져다주면 비명을 지르고, 몸을 비틀면서 움직임"
/// </summary>
public class Stage4_GhoulCreature : CreatureBase
{
	[Header("Ghoul Settings")]
	[SerializeField] private Transform tablePosition;
	[SerializeField] private Animator animator;

	private bool _hasScreamed = false;

	protected override void Start()
	{
		base.Start();
		transform.position = tablePosition.position;
	}

	protected override void UpdateBehavior()
	{
		// 플레이어 응시
		transform.LookAt(_player.transform);
	}

	public void TriggerScream()
	{
		if (_hasScreamed) return;

		_hasScreamed = true;

		// 비명
		var audioManager = GameServices.Audio;
		audioManager?.PlaySFX("ghoul_scream");

		// 애니메이션
		animator?.SetTrigger("Scream");

		Debug.Log("[Ghoul] 비명!!!");
	}
}