using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    public RectTransform rectTransform;
    [SerializeField]
    public float xSizeNew, ySizeNew;

    private float xSizeDefault, ySizeDefault;

    private LTRect ltRect;

    private void Start()
    {
        xSizeDefault = rectTransform.sizeDelta.x;
        ySizeDefault = rectTransform.sizeDelta.y;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.sizeDelta = new Vector2(xSizeNew, ySizeNew);
        Debug.Log(gameObject.name + " hovered");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.sizeDelta = new Vector2(xSizeDefault, ySizeDefault);
    }
}
