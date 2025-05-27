using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScript : MonoBehaviour
{
    public Sprite I;

    public void EndingScene()
    {
        MainSoundManager.instance.ChangeBGM("GIGACHAD");
        DialogueManager.instance.setImage(I);
        StartCoroutine(WaitAndLoadMainMenu(4f)); 
    }

    private IEnumerator WaitAndLoadMainMenu(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간(delay)만큼 기다림

        if (GameManager.instance != null)
            GameManager.instance.off();
        if (DialogueManager.instance != null)
            DialogueManager.instance.off();
        if (MainSoundManager.instance != null)
            MainSoundManager.instance.off();
        if (VirtualInputManager.Instance != null)
            VirtualInputManager.Instance.off();
        SceneManager.LoadScene("MainMenu");
    }
}