using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageOneSelect : MonoBehaviour
{
   

    public void LoadStageOneBoss()
    {
        GameManager.setIndex(7);
        SceneManager.LoadScene("SampleScene");
    }

}