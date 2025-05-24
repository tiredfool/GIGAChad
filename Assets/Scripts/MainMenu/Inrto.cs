using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Intro: MonoBehaviour
{

    public GameObject FadePannel;
    float fadeCount = 255;

    private void Start()
    {

        StartCoroutine(FadeOutCoroutine());
       
    }
    private void Update()
    {
        if(fadeCount <= 0) FadePannel.gameObject.SetActive(false);

    }
    IEnumerator FadeOutCoroutine()
    {

        FadePannel.SetActive(true);
        for (float f = 2f; f > 0; f -= 0.01f)
        {
            Color c = FadePannel.GetComponent<Image>().color;
            c.a = f;
            FadePannel.GetComponent<Image>().color = c;
            yield return null;
        }
        yield return new WaitForSeconds(1);
        //Destroy(FadePannel);
        FadePannel.SetActive(false);

    }
}
