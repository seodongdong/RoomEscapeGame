using UnityEngine;

public class EnemyChaser : MonoBehaviour, IEnemy
{
	[SerializeField] private float chaseSpeed = 6f;
	[SerializeField] private float attackRange = 2f;
	[SerializeField] private int attackDamage = 20;

	private Transform _target;
	private bool _isChasing;

	public bool IsChasing => _isChasing;

	public void Chase(Transform target)
	{
		_target = target;
		_isChasing = true;
	}

	public void StopChasing()
	{
		_isChasing = false;
		_target = null;
	}

	public void AttackTarget(IPlayer target)
	{
		target.TakeDamage(attackDamage);
		Debug.Log("적이 플레이어를 공격했습니다!");
	}

	private void Update()
	{
		if (!_isChasing || _target == null) return;

		Vector3 direction = (_target.position - transform.position).normalized;
		transform.position += direction * chaseSpeed * Time.deltaTime;
		transform.LookAt(_target);

		float distance = Vector3.Distance(transform.position, _target.position);
		if (distance <= attackRange)
		{
			if (_target.TryGetComponent<IPlayer>(out var player))
			{
				AttackTarget(player);
			}
		}
	}
}