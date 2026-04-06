using UnityEngine;

public class FoodDrop : MonoBehaviour
{
	private int _calories;
	private bool _isGood;
	private FoodMiniGame _miniGame;

	public void Initialize(int calories, bool isGood)
	{
		_calories = calories;
		_isGood = isGood;
		_miniGame = FindAnyObjectByType<FoodMiniGame>();
	}

	private void Update()
	{
		transform.position += Vector3.down * 5f * Time.deltaTime;

		if (transform.position.y < -10f)
		{
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			_miniGame?.CollectFood(_calories, _isGood);
			Destroy(gameObject);
		}
	}
}