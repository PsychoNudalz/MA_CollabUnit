using UnityEngine;
using UnityEngine.EventSystems;


public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    public RectTransform rectTransform;
    [SerializeField]
    public float xSizeDefault, ySizeDefault, xSizeNew, ySizeNew;

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.sizeDelta = new Vector2(xSizeNew, ySizeNew);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.sizeDelta = new Vector2(xSizeDefault, ySizeDefault);
    }
}
