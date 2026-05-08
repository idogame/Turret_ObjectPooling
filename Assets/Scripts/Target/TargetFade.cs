using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFader : MonoBehaviour
{
    [SerializeField] private RawImage image;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        FadeOut();
    }

    public void FadeIn()
    {
        StartFade(0f, 1f);
    }

    public void FadeOut()
    {
        StartFade(1f, 0f);
    }

    private void StartFade(float from, float to)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(from, to));
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        // 이미지 참조가 없는 경우 예외 방지
        if (image == null) yield break;

        float elapsed = 0f;
        Color tempColor = image.color;

        // 초기값 설정
        tempColor.a = from;
        image.color = tempColor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            // 0으로 나누기 방지 및 클램핑
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            tempColor.a = Mathf.Lerp(from, to, t);
            image.color = tempColor;

            yield return null;
        }

        // 최종값 보정
        tempColor.a = to;
        image.color = tempColor;
        fadeCoroutine = null;
    }
}