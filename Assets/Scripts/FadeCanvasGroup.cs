using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeCanvasGroup : MonoBehaviour
{
    public float fadeDuration = 0.4f;

    private void OnEnable()
    {
        GetComponent<CanvasGroup>().alpha = 1.0f;
        StartCoroutine(DoFade(GetComponent<CanvasGroup>()));
    }

    public IEnumerator DoFade(CanvasGroup copied_Notification)
    {
        float counter = 0f;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            copied_Notification.alpha = Mathf.Lerp(1f, 0f, counter / fadeDuration);
            yield return null;
        }
        copied_Notification.gameObject.SetActive(false);
    }
}
