using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneMain : MonoBehaviour
{
    

    public void MainMenuScene()
    {
        //if(SwitchZone.Instance!=null)
        //    SwitchZone.Instance.off();
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
