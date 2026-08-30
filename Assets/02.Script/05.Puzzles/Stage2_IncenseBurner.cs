using UnityEngine;

/// <summary>
/// 2스테이지 제단 향로 — 퍼즐 화면 안의 조작 버튼 2종.
///
/// [기획서]
/// 향로 A : 퍼즐 리셋   (현재 단계만 초기화 + 불빛 순서 새로 랜덤 생성)
/// 향로 B : 퍼즐 나가기 (현재 단계만 초기화 + 불빛 순서 새로 랜덤 생성)
///
/// [퍼즐 밖에서는]
/// F키 상호작용 시 살펴보기용 단서로 동작합니다.
/// 같은 오브젝트에 ObjectViewer3D가 있으면 3D 뷰어를 열고,
/// 없으면 대사만 출력합니다. (기존 AltarIncense와 동일한 동작)
///
/// [씬 설정]
/// 1. 향로 오브젝트에 이 스크립트 + Collider(IsTrigger: false) 부착
/// 2. role을 ResetPuzzle(향로 A) 또는 ExitPuzzle(향로 B)로 설정
/// 3. puzzle 슬롯에 Stage2_LightSequencePuzzle 연결
/// 4. (선택) 같은 오브젝트에 ObjectViewer3D 부착
/// </summary>
public class Stage2_IncenseBurner : InteractableBase
{
	public enum IncenseRole
	{
		ResetPuzzle,  // 향로 A
		ExitPuzzle    // 향로 B
	}

	[Header("역할")]
	[SerializeField] private IncenseRole role = IncenseRole.ResetPuzzle;

	[Header("퍼즐 연결")]
	[SerializeField] private Stage2_LightSequencePuzzle puzzle;

	[Header("단서 정보 (퍼즐 밖 F키)")]
	[SerializeField] private string clueId = "stage2_incense";
	[SerializeField] private string clueName = "향로";
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string clueDialogue = "향이 완전히 다 탔다... 얼마나 오래 피운 걸까.";

	[Header("퍼즐 안 클릭 시 대사")]
	[TextArea(2, 4)][SerializeField] private string resetDialogue = "...처음부터 다시 해보자.";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "일단 나가서 다시 확인해보자.";

	[Header("효과음")]
	[SerializeField] private string clickSFX = "incense_click";

	private ObjectViewer3D _viewer;
	private bool _clueRegistered = false;

	private void Awake()
	{
		_viewer = GetComponent<ObjectViewer3D>();
	}

	// ── 퍼즐 밖: F키 상호작용 (살펴보기용 단서) ──────────────

	public override string InteractionPrompt => $"[F] {clueName} 살펴보기";

	public override bool CanInteract(IPlayer player) => true;

	protected override void OnInteract(IPlayer player)
	{
		if (!_clueRegistered)
		{
			_clueRegistered = true;
			ClueRegistrar.RegisterClueOnly(clueId);
		}

		if (_viewer != null) _viewer.Interact(player);
		else GameServices.UI?.ShowDialogue(speaker, clueDialogue);
	}

	// ── 퍼즐 안: 마우스 클릭 ─────────────────────────────────

	private void OnMouseDown()
	{
		if (GameManager.Instance == null) return;
		if (GameManager.Instance.CurrentState != GameState.Puzzle) return;
		if (puzzle == null) return;

		if (!string.IsNullOrEmpty(clickSFX))
			GameServices.Audio?.PlaySFX(clickSFX);

		if (role == IncenseRole.ResetPuzzle)
		{
			GameServices.UI?.ShowDialogue(speaker, resetDialogue);
			puzzle.ResetCurrentStageFromIncense();
		}
		else
		{
			GameServices.UI?.ShowDialogue(speaker, exitDialogue);
			puzzle.ExitPuzzle();
		}
	}
}