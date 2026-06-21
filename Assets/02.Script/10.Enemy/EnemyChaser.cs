using UnityEngine;

/// <summary>
/// 범인 AI
/// 기획서: "무기를 들고 쫓아옴"
/// </summary>
public class EnemyChaser : MonoBehaviour, IEnemy
{
	[Header("Chase Settings")]
	[SerializeField] private float chaseSpeed = 6f;
	[SerializeField] private float attackRange = 2f;
	[SerializeField] private int attackDamage = 20;

	[Header("Attack Cooldown")]
	[SerializeField] private float attackCooldown = 1f;

	private Transform _target;
	private bool _isChasing;
	private float _lastAttackTime;

	public bool IsChasing => _isChasing;

	public void Chase(Transform target)
	{
		_target = target;
		_isChasing = true;

		Debug.Log("[Enemy] 추격 시작");
	}

	public void StopChasing()
	{
		_isChasing = false;
		_target = null;

		Debug.Log("[Enemy] 추격 정지");
	}

	public void AttackTarget(IPlayer target)
	{
		target.TakeDamage(attackDamage);
		Debug.Log("[Enemy] 플레이어 공격!");
	}

	private void Update()
	{
		if (!_isChasing || _target == null) return;

		Vector3 direction = (_target.position - transform.position).normalized;
		transform.position += direction * chaseSpeed * Time.deltaTime;
		transform.LookAt(_target);

		float distance = Vector3.Distance(transform.position, _target.position);
		if (distance <= attackRange && Time.time >= _lastAttackTime + attackCooldown)
		{
			if (_target.TryGetComponent<IPlayer>(out var player))
			{
				AttackTarget(player);
				_lastAttackTime = Time.time;
			}
		}
	}
}