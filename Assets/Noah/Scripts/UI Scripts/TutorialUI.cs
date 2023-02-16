using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public class TutorialUI : MonoBehaviour
{
    // Start is called before the first frame update

    public RectTransform tut1, tut2, tut3;

    public PauseMenu pm;
    
    void Start()
    {
        pm.canBePaused = false;
        Cursor.visible = true;
        
        Time.timeScale = 0f;
        LeanTween.scale(gameObject.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);

        LeanTween.scale(tut1, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true).setDelay(2f);
        LeanTween.alpha(tut1, 1, 0.5f).setIgnoreTimeScale(true).setDelay(2f);
        
        LeanTween.scale(tut2, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true).setDelay(5f);
        LeanTween.alpha(tut2, 1, 0.5f).setIgnoreTimeScale(true).setDelay(5f);
        
        LeanTween.scale(tut3, new Vector3(1, 1, 1), 0.3f).setEase(LeanTweenType.easeInOutCubic)
            .setIgnoreTimeScale(true).setDelay(8f);
        LeanTween.alpha(tut3, 1, 0.5f).setIgnoreTimeScale(true).setDelay(8f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void StartGame()
    {
        Time.timeScale = 1f;
        LeanTween.scale(gameObject.GetComponent<RectTransform>(), new Vector3(0, 0, 0), 0.3f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true);
        pm.canBePaused = true;
    }
    
    
}
