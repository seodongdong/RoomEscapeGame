using UnityEngine;

/// <summary>
/// 2스테이지: 그림자 크리처
/// 기획서: "작은 방 앞에 있던 인영크리쳐가 맵 뒤쪽 중앙으로 이동"
/// "가만히 서서 플레이어를 빤히 응시하기만 함"
/// </summary>
public class Stage2_ShadowCreature : CreatureBase
{
	[Header("Shadow Settings")]
	[SerializeField] private Transform initialPosition;
	[SerializeField] private Transform finalPosition;

	private bool _hasMoved = false;

	protected override void UpdateBehavior()
	{
		// 플레이어 응시
		transform.LookAt(_player.transform);
	}

	public void MoveToFinalPosition()
	{
		if (_hasMoved) return;

		_hasMoved = true;
		transform.position = finalPosition.position;

		Debug.Log("[ShadowCreature] 최종 위치로 이동");
	}
}