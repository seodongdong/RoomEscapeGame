using UnityEngine;
using System.Collections;

/// <summary>
/// 2스테이지: 점프스케어 트리거
///
/// [기획서 내용]
/// "퍼즐 다 푼 후 관 앞에서 뒤를 돌아보면 방석에 사람 인영과 큰 박수소리"
///
/// [동작 방식]
/// - 퍼즐 해결 후(_puzzleReference.IsSolved == true) 플레이어가 이 트리거에 진입 시
///   크리처 인영 오브젝트를 방석 위에 잠깐 활성화 + 박수 SFX
/// - 1회만 발동
///
/// [씬 배치]
/// 관 앞 방석 구역에 BoxCollider(IsTrigger)를 가진 오브젝트로 배치합니다.
/// </summary>
public class Stage2_JumpscareTrigger : MonoBehaviour
{
	[Header("퍼즐 연결 (해결 후에만 발동)")]
	[SerializeField] private Stage2_AltarCandyPuzzle puzzleReference;

	[Header("점프스케어 오브젝트")]
	[SerializeField] private GameObject jumpscareCreatureVisual;  // 방석 위에 나타날 인영
	[SerializeField] private float visibleDuration = 1.5f;        // 몇 초간 보이게 할지

	[Header("오디오")]
	[SerializeField] private string jumpscareSFX = "applause_loud"; // AudioManager SFX ID

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string jumpscareDialogue = "!!";

	private bool _hasTriggered = false;

	private void Awake()
	{
		// 인영 비주얼은 처음에 비활성화
		if (jumpscareCreatureVisual != null)
			jumpscareCreatureVisual.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_hasTriggered) return;
		if (!other.CompareTag("Player")) return;

		// 퍼즐이 해결된 경우에만 발동
		if (puzzleReference == null || !puzzleReference.IsSolved) return;

		_hasTriggered = true;
		StartCoroutine(PlayJumpscare());
	}

	private IEnumerator PlayJumpscare()
	{
		// 인영 활성화
		if (jumpscareCreatureVisual != null)
			jumpscareCreatureVisual.SetActive(true);

		// 박수 소리
		var audioManager = GameServices.Audio;
		audioManager?.PlaySFX(jumpscareSFX);

		// 대사 출력
		var uiManager = GameServices.UI;
		uiManager?.ShowDialogue(speaker, jumpscareDialogue);

		Debug.Log("[Stage2 Jumpscare] 방석 인영 등장!");

		// 일정 시간 후 비활성화
		yield return new WaitForSeconds(visibleDuration);

		if (jumpscareCreatureVisual != null)
			jumpscareCreatureVisual.SetActive(false);
	}
}