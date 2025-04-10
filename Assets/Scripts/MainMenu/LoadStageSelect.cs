using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadStageSelect: MonoBehaviour
{


    public void StageSelectScene()
    {
        SceneManager.LoadScene("StageSelect");
    }


}

