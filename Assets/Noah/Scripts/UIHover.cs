using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    public RectTransform rectTransform;
    [SerializeField]
    public float xSizeDefault, ySizeDefault, xSizeNew, ySizeNew;

    private LTRect ltRect;
    

    public void OnPointerEnter(PointerEventData eventData)
    {
        //LeanTween.scale(ltRect, new Vector2(1.3f,1.3f), 0.3f);
        rectTransform.sizeDelta = new Vector2(xSizeNew, ySizeNew);
        Debug.Log(gameObject.name + " hovered");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //LeanTween.scale(ltRect, new Vector2(1,1), 0.3f);
        rectTransform.sizeDelta = new Vector2(xSizeDefault, ySizeDefault);

    }
}
