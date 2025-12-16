using UnityEngine;

public class Stage2_ShadowCreature : CreatureBase
{
	[Header("Shadow Settings")]
	[SerializeField] private float patrolRadius = 5f;
	[SerializeField] private Transform[] patrolPoints;

	private int _currentPatrolIndex;

	protected override void UpdateBehavior()
	{
		if (patrolPoints == null || patrolPoints.Length == 0) return;

		// 순찰 지점 배회
		Transform targetPoint = patrolPoints[_currentPatrolIndex];

		Vector3 direction = (targetPoint.position - transform.position).normalized;
		transform.position += direction * moveSpeed * Time.deltaTime;

		// 도착 시 다음 지점으로
		if (Vector3.Distance(transform.position, targetPoint.position) < 1f)
		{
			_currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
		}
	}
}