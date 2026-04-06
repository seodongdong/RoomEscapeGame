using UnityEngine;

public class Stage3_DollCreature : CreatureBase
{
	[Header("Ghost Settings")]
	[SerializeField] private float ceilingHeight = 5f;
	[SerializeField] private bool crawlOnCeiling = true;

	protected override void UpdateBehavior()
	{
		if (crawlOnCeiling)
		{
			// 천장을 기어다님
			Vector3 targetPos = _player.transform.position;
			targetPos.y = ceilingHeight;

			Vector3 direction = (targetPos - transform.position).normalized;
			transform.position += direction * moveSpeed * Time.deltaTime;

			// 플레이어 아래로 내려다보기
			transform.LookAt(_player.transform);
		}
		else
		{
			// 일반 추적
			Vector3 direction = (_player.transform.position - transform.position).normalized;
			transform.position += direction * moveSpeed * Time.deltaTime;
		}
	}
}