using UnityEngine;

/// <summary>
/// 2스테이지 퍼즐 화면의 촛불 1개.
/// 단계 클리어 현황을 표시합니다. (1단계 → 왼쪽, 2단계 → 오른쪽, 3단계 → 가운데)
///
/// [씬 설정]
/// 촛대 위 초 오브젝트에 부착하고, flameObject에 불꽃 파티클/메시를 연결하세요.
/// </summary>
public class Stage2_Candle : MonoBehaviour
{
	[Header("불꽃 표현")]
	[SerializeField] private GameObject flameObject;
	[SerializeField] private Light flameLight;
	[SerializeField] private ParticleSystem flameParticle;

	[Header("효과음")]
	[SerializeField] private string igniteSFX = "candle_light";

	public bool IsLit { get; private set; }

	private void Awake() => SetLit(false, playSFX: false);

	public void SetLit(bool lit, bool playSFX = true)
	{
		IsLit = lit;

		if (flameObject != null) flameObject.SetActive(lit);
		if (flameLight != null) flameLight.enabled = lit;

		if (flameParticle != null)
		{
			if (lit) flameParticle.Play();
			else flameParticle.Stop();
		}

		if (lit && playSFX && !string.IsNullOrEmpty(igniteSFX))
			GameServices.Audio?.PlaySFX(igniteSFX);
	}
}