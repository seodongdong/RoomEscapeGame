using UnityEngine;

/// <summary>
/// 2스테이지: 여자아이 영정사진 액자.
///
/// [기획서 — 표정 연출]
///   퍼즐 시작 전 / 대기 중 : 우울한 표정        (stage 0)
///   각목 클릭 중           : 우울한 표정 유지    (stage 0 유지)
///   오답 판정 시           : 반응 없음           (변경하지 않음)
///   1단계 클리어           : 조금 더 어두워짐    (stage 1)
///   2단계 클리어           : 더 어두워짐         (stage 2)
///   3단계 클리어           : 완전히 우울한 표정  (stage 3, 기괴)
///
/// 액자는 퍼즐 화면 밖(맵)에서 확인할 수 있도록 배치합니다.
///
/// [씬 설정]
/// SpriteRenderer 방식과 Material 교체 방식 둘 다 지원합니다.
/// - photoSpriteRenderer + expressionSprites (4개)  → 2D 사진 스프라이트
/// - photoRenderer + expressionMaterials (4개)      → 3D 액자 머티리얼
/// 둘 중 편한 쪽만 채워도 동작합니다.
/// </summary>
public class Stage2_PortraitFrame : MonoBehaviour
{
	[Header("스프라이트 방식")]
	[SerializeField] private SpriteRenderer photoSpriteRenderer;
	[Tooltip("0=기본(우울) / 1=1단계 / 2=2단계 / 3=3단계(완전히 우울·기괴)")]
	[SerializeField] private Sprite[] expressionSprites = new Sprite[4];

	[Header("머티리얼 방식")]
	[SerializeField] private Renderer photoRenderer;
	[Tooltip("0=기본(우울) / 1=1단계 / 2=2단계 / 3=3단계(완전히 우울·기괴)")]
	[SerializeField] private Material[] expressionMaterials = new Material[4];

	[Header("효과음")]
	[SerializeField] private string expressionChangeSFX = "";

	private int _currentStage = -1;

	private void Start()
	{
		SetExpressionStage(0);
	}

	/// <summary>
	/// 표정 단계 설정. 0 = 기본(우울), 1~3 = 단계 클리어 상태.
	/// </summary>
	public void SetExpressionStage(int stage)
	{
		if (stage == _currentStage) return;
		_currentStage = Mathf.Clamp(stage, 0, 3);

		if (photoSpriteRenderer != null &&
			expressionSprites != null &&
			_currentStage < expressionSprites.Length &&
			expressionSprites[_currentStage] != null)
		{
			photoSpriteRenderer.sprite = expressionSprites[_currentStage];
		}

		if (photoRenderer != null &&
			expressionMaterials != null &&
			_currentStage < expressionMaterials.Length &&
			expressionMaterials[_currentStage] != null)
		{
			photoRenderer.material = expressionMaterials[_currentStage];
		}

		if (_currentStage > 0 && !string.IsNullOrEmpty(expressionChangeSFX))
			GameServices.Audio?.PlaySFX(expressionChangeSFX);

		Debug.Log($"[PortraitFrame] 표정 단계 → {_currentStage}");
	}

	/// <summary>기본(우울) 표정으로 복귀. 오답 리셋 시 호출됩니다.</summary>
	public void ResetToDefault() => SetExpressionStage(0);
}