using UnityEngine;

/// <summary>
/// 1스테이지: 인형 크리처
/// 기획서: "주인공의 주변을 맴돌면서 딱히 아무런 역할을 하지않음"
/// "가오나시처럼 '.................'"
/// </summary>
public class Stage1_DollCreature : CreatureBase
{
	[Header("Doll Settings")]
	[SerializeField] private float orbitRadius = 3f;
	[SerializeField] private float orbitSpeed = 1f;

	private float _orbitAngle = 0f;

	protected override void UpdateBehavior()
	{
		// 플레이어 주변을 맴돎
		_orbitAngle += orbitSpeed * Time.deltaTime;

		Vector3 offset = new Vector3(
			Mathf.Cos(_orbitAngle) * orbitRadius,
			0f,
			Mathf.Sin(_orbitAngle) * orbitRadius
		);

		transform.position = _player.transform.position + offset;
		transform.LookAt(_player.transform);
	}
}