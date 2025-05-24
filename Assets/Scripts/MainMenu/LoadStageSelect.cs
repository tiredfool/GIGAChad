using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadStageSelect: MonoBehaviour
{


    public void StageSelectScene()
    {
        SwitchZone.Instance.off();
        GameManager.instance.off();
        DialogueManager.instance.off();
        MainSoundManager.instance.off();
        SceneManager.LoadScene("StageSelect");
        Debug.Log("Å¬¸¯µÊ");
    }


}

