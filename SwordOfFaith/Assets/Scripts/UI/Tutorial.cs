using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Tutorial : MonoBehaviour
{
    [SerializeField] private Sprite move;
    [SerializeField] private Sprite attack;
    [SerializeField] private Sprite swap;

    private Image image;

    private const int waitTime = 5;
    private const float fadeTime = 0.5f;
    private float p, t;
    private Color imageColor;

    private void Awake()
    {
        if ((image = GetComponent<Image>()) != null)
        {
            imageColor = image.color;
            StartCoroutine(Show());
        }
    }

    public IEnumerator Show()
    {
        p = 0;
        t = 0;
        yield return new WaitForSeconds(1);
        while (t < fadeTime)
        {
            FadeIN();
            yield return new WaitForEndOfFrame();
        }
        image.sprite = move;
        yield return new WaitForSeconds(waitTime);
        while (t > 0)
        {
            FadeOUT();
            yield return new WaitForEndOfFrame();
        }
        image.sprite = swap;
        while (t < fadeTime)
        {
            FadeIN();
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(waitTime);
        while (t > 0)
        {
            FadeOUT();
            yield return new WaitForEndOfFrame();
        }
        image.sprite = attack;
        while (t < fadeTime)
        {
            FadeIN();
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(waitTime);
        while (t > 0)
        {
            FadeOUT();
            yield return new WaitForEndOfFrame();
        }
    }

    public void FadeIN()
    {
        t += Time.deltaTime;
        p = t / fadeTime;
        imageColor.a = p;
        image.color = imageColor;
    }

    public void FadeOUT()
    {
        t -= Time.deltaTime;
        p = t / fadeTime;
        imageColor.a = p;
        image.color = imageColor;
    }

}
