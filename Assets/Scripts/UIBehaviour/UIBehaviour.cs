using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;

public class UIBehaviour:MonoBehaviour {

    static bool _firstBoot = true;

    public Image FadeImage;
    public RectTransform Title;
    public GameObject Buttons;
    public Camera UICamera;
    public GameObject RigidbodyFPSController;
    public Canvas Canvas;

    bool _isUIMode = true;

    public void Awake()
    {
        if (_firstBoot)
        {
            SetTitleBehind();
            _firstBoot = false;
        }
    }

    public void Start()
    {
        
        StartCoroutine(FadeAndThenCallback(new Color(1,1,1,0), 1.5f, SetTitleInFront, true));
    }

    public void Update()
    {
        if (Input.GetButtonDown("Cancel") && !_isUIMode)
        {
            StartCoroutine(FadeAndThenCallback(Color.white, 0.75f, SwitchToUIMode, false));
        }
    }

    public void Explore()
    {
        SetTitleBehind();
        StartCoroutine(FadeAndThenCallback(Color.white, 2f, SwitchToFPSMode, false));
    }

    public void Regenerate()
    {
        StartCoroutine(FadeAndThenCallback(Color.white, 2f, RestartScene, false));
    }

    public void Quit()
    {
        SetTitleBehind();
        StartCoroutine(FadeAndThenCallback(Color.white, 1f, Application.Quit, false));
        
    }

    IEnumerator FadeAndThenCallback(Color color, float time, Action callback, bool disableAfterwards )
    {
        FadeImage.gameObject.SetActive(true);
        var currentColor = FadeImage.color;
        var targetColor = color;

        var gradient = new Gradient();
        var colorKeys = new GradientColorKey[] { new GradientColorKey(currentColor, 0f), new GradientColorKey(targetColor, 1f) };
        var alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(currentColor.a, 0f), new GradientAlphaKey(color.a, 1f) };

        gradient.SetKeys(colorKeys, alphaKeys);

        var currentTime = 0f;


        while (currentTime < time)
        {
            currentTime += Time.deltaTime;

            FadeImage.color = gradient.Evaluate(Mathf.InverseLerp(0, time, currentTime));
            yield return null;
        }

        if (disableAfterwards)
        {
            FadeImage.gameObject.SetActive(false);
        }

        if(callback != null)
        { callback(); }

        
    }

    void RestartScene()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    void SwitchToFPSMode()
    {
        if (_isUIMode)
        {
            RigidbodyFPSController.SetActive(true);
            UICamera.gameObject.SetActive(false);
            Buttons.SetActive(false);
            Title.gameObject.SetActive(false);
            //Canvas.enabled = false;
            StartCoroutine(FadeAndThenCallback(new Color(1, 1, 1, 0), 1.5f, null, true));

            _isUIMode = false;
        }

        
    }

    void SwitchToUIMode()
    {
        if (!_isUIMode)
        {
            RigidbodyFPSController.SetActive(false);
            UICamera.gameObject.SetActive(true);
            Buttons.SetActive(true);
            Title.gameObject.SetActive(true);
            //Canvas.enabled = true;
            StartCoroutine(FadeAndThenCallback(new Color(1, 1, 1, 0), 1f, SetTitleInFront, true));
            _isUIMode = true;
        }
        
    }

    void SetTitleInFront()
    {
        
        int index = Title.transform.GetSiblingIndex();
        Title.transform.SetSiblingIndex(index + 1);
    }

    void SetTitleBehind()
    {
        int index = FadeImage.transform.GetSiblingIndex();
        FadeImage.transform.SetSiblingIndex(index + 1);
    }


}
