// 슬롯 버튼에 추가
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonDebug : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log($">>> 마우스 올림: {gameObject.name}");
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log($">>> 클릭됨: {gameObject.name}");
	}
}
