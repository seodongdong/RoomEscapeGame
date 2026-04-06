using UnityEngine;

public class Stage1_DollCreature : CreatureBase
{
	[Header("Doll Settings")]
	[SerializeField] private float slowSpeed = 1f;

	protected override void UpdateBehavior()
	{
		// 플레이어를 천천히 따라감
		Vector3 direction = (_player.transform.position - transform.position).normalized;
		transform.position += direction * slowSpeed * Time.deltaTime;

		// 플레이어 바라보기
		transform.LookAt(_player.transform);
	}
}