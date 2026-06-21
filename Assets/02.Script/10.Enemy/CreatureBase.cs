using UnityEngine;
using System.Collections;

/// <summary>
/// 크리처 베이스 클래스
/// </summary>
public abstract class CreatureBase : MonoBehaviour
{
	[Header("Creature Settings")]
	[SerializeField] protected int stageNumber;
	[SerializeField] protected float moveSpeed = 2f;
	[SerializeField] protected bool canJumpscare = true;

	[Header("Detection")]
	[SerializeField] protected float detectionRange = 10f;
	[SerializeField] protected float jumpscareRange = 2f;

	protected Player _player;
	protected bool _hasJumpscared;
	protected bool _isActive = true;

	protected virtual void Start()
	{
		_player = GameServices.Player;
	}

	protected virtual void Update()
	{
		if (!_isActive || _player == null) return;

		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		if (distanceToPlayer < jumpscareRange && !_hasJumpscared)
		{
			TriggerJumpscare();
		}
		else if (distanceToPlayer < detectionRange)
		{
			UpdateBehavior();
		}
	}

	protected abstract void UpdateBehavior();

	protected virtual void TriggerJumpscare()
	{
		if (!canJumpscare || _hasJumpscared) return;

		_hasJumpscared = true;

		var audioManager = GameServices.Audio;
		audioManager?.PlaySFX("jumpscare");

		Debug.Log($"[{gameObject.name}] 점프스케어!");

		StartCoroutine(DisappearAfterJumpscare());
	}

	private IEnumerator DisappearAfterJumpscare()
	{
		yield return new WaitForSeconds(1f);
		_isActive = false;
		gameObject.SetActive(false);
	}

	public void SetActive(bool active)
	{
		_isActive = active;
		gameObject.SetActive(active);
	}
}