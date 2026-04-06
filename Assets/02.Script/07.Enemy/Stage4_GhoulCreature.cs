using UnityEngine;

public class Stage4_GhoulCreature : CreatureBase
{
	[Header("Ghoul Settings")]
	[SerializeField] private Transform hidePosition; // 냉장고 뒤
	[SerializeField] private float stareDistance = 5f;

	private bool _isHiding = true;

	protected override void Start()
	{
		base.Start();

		if (hidePosition != null)
		{
			transform.position = hidePosition.position;
		}
	}

	protected override void UpdateBehavior()
	{
		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if (_isHiding)
		{
			// 플레이어가 가까이 오면 나타남
			if (distanceToPlayer < stareDistance)
			{
				_isHiding = false;
			}

			// 숨어서 플레이어 응시
			transform.LookAt(_player.transform);
		}
		else
		{
			// 천천히 다가옴
			Vector3 direction = (_player.transform.position - transform.position).normalized;
			transform.position += direction * moveSpeed * Time.deltaTime;
			transform.LookAt(_player.transform);
		}
	}
}