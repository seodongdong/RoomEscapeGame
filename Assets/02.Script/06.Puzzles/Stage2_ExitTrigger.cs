using UnityEngine;

/// <summary>
/// 2스테이지: 퇴장 트리거
///
/// [기획서 내용]
/// "아이템 획득 후 똑같이 아저씨의 시선을 느끼며 퇴실
///  (이때도 아저씨는 상호작용불가이지만, 입만 활짝 웃고 있는걸로 해도 괜찮을 듯;
///  납치를 성공했으니)"
///
/// [씬 배치]
/// 출구 문 앞/안쪽에 BoxCollider(IsTrigger)를 가진 오브젝트로 배치.
/// 플레이어가 통과하면 크리처 웃음 연출 발동.
/// </summary>
public class Stage2_ExitTrigger : MonoBehaviour
{
	[Header("크리처 연결")]
	[SerializeField] private Stage2_ShadowCreature shadowCreature;

	[Header("대사 설정")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string exitDialogue = "...뒤에서 시선이 느껴진다.";

	private bool _hasTriggered = false;

	private void OnTriggerEnter(Collider other)
	{
		if (_hasTriggered) return;
		if (!other.CompareTag("Player")) return;

		_hasTriggered = true;

		// 크리처 웃음 연출
		if (shadowCreature != null)
			shadowCreature.TriggerExitSmile();

		// 대사 출력
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, exitDialogue);

		Debug.Log("[Stage2_ExitTrigger] 퇴장 연출 발동");
	}
}