using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 씬 전환 관리
/// 페이드 효과 포함
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
	private static SceneTransitionManager _instance;
	public static SceneTransitionManager Instance => _instance;

	[Header("Fade Settings")]
	[SerializeField] private Image fadeImage;
	[SerializeField] private float fadeDuration = 1f;

	[Header("Loading")]
	[SerializeField] private GameObject loadingPanel;
	[SerializeField] private Slider loadingBar;

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);
	}

	/// <summary>
	/// 씬 전환 (페이드 효과)
	/// </summary>
	public void LoadScene(string sceneName)
	{
		StartCoroutine(LoadSceneCoroutine(sceneName));
	}

	/// <summary>
	/// 씬 전환 (인덱스)
	/// </summary>
	public void LoadScene(int sceneIndex)
	{
		StartCoroutine(LoadSceneCoroutine(sceneIndex));
	}

	private IEnumerator LoadSceneCoroutine(string sceneName)
	{
		// 페이드 아웃
		yield return StartCoroutine(FadeOut());

		// 로딩 패널 표시
		if (loadingPanel != null)
		{
			loadingPanel.SetActive(true);
		}

		// 씬 로드
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		asyncLoad.allowSceneActivation = false;

		// 로딩 바 업데이트
		while (!asyncLoad.isDone)
		{
			float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

			if (loadingBar != null)
			{
				loadingBar.value = progress;
			}

			if (asyncLoad.progress >= 0.9f)
			{
				asyncLoad.allowSceneActivation = true;
			}

			yield return null;
		}

		// 로딩 패널 숨김
		if (loadingPanel != null)
		{
			loadingPanel.SetActive(false);
		}

		// 페이드 인
		yield return StartCoroutine(FadeIn());
	}

	private IEnumerator LoadSceneCoroutine(int sceneIndex)
	{
		yield return StartCoroutine(FadeOut());

		SceneManager.LoadScene(sceneIndex);

		yield return StartCoroutine(FadeIn());
	}

	private IEnumerator FadeOut()
	{
		if (fadeImage == null) yield break;

		fadeImage.gameObject.SetActive(true);
		float elapsed = 0f;
		Color color = fadeImage.color;

		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			color.a = elapsed / fadeDuration;
			fadeImage.color = color;
			yield return null;
		}

		color.a = 1f;
		fadeImage.color = color;
	}

	private IEnumerator FadeIn()
	{
		if (fadeImage == null) yield break;

		float elapsed = 0f;
		Color color = fadeImage.color;

		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			color.a = 1f - (elapsed / fadeDuration);
			fadeImage.color = color;
			yield return null;
		}

		color.a = 0f;
		fadeImage.color = color;
		fadeImage.gameObject.SetActive(false);
	}

	/// <summary>
	/// 즉시 씬 전환 (페이드 없음)
	/// </summary>
	public void LoadSceneImmediate(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}