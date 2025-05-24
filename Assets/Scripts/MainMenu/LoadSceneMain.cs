using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneMain : MonoBehaviour
{
    

    public void MainMenuScene()
    {
        SwitchZone.Instance.off();
        GameManager.instance.off();
        DialogueManager.instance.off();
        MainSoundManager.instance.off();
        SceneManager.LoadScene("MainMenu");
    }


}
