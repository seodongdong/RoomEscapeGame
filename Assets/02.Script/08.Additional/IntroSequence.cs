using UnityEngine;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Player player;
	[SerializeField] private Girl girl;
	[SerializeField] private Transform doorPosition;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string wakeUpDialogue = "...여기가 어디지?";
	[TextArea(2, 5)]
	[SerializeField] private string doorLockedDialogue = "문이... 안 열려!";

	[Header("Camera")]
	[SerializeField] private Camera introCamera;

	private IUIManager _uiManager;

	private void Start()
	{
		_uiManager = FindAnyObjectByType<UIManager>();

		// 인트로 시작
		StartCoroutine(PlayIntro());
	}

	private IEnumerator PlayIntro()
	{
		// 플레이어 조작 비활성화
		if (player != null)
		{
			player.enabled = false;
		}

		// 상태 변경
		GameManager.Instance.StateManager.ChangeState(GameState.MainMenu);

		// 1. 비디오 화면 효과 (옵션)
		yield return new WaitForSeconds(2f);

		// 2. 깨어남
		_uiManager?.ShowDialogue(speaker, wakeUpDialogue);
		yield return new WaitForSeconds(3f);

		// 3. 플레이어 활성화
		if (player != null)
		{
			player.enabled = true;
		}

		// 인트로 카메라 비활성화
		if (introCamera != null)
		{
			introCamera.gameObject.SetActive(false);
		}

		// 4. 대문으로 유도 (옵션)
		yield return new WaitForSeconds(2f);

		// 5. 대문 시도
		if (doorPosition != null)
		{
			// 플레이어가 대문 근처에 가면...
			// (실제로는 Trigger로 구현)
		}

		// 6. 소녀 등장
		yield return new WaitForSeconds(1f);
		girl?.FirstMeeting();

		// 7. 게임 시작
		GameManager.Instance.StateManager.ChangeState(GameState.Playing);
	}
}