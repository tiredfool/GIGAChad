using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageTwoSelect : MonoBehaviour
{




    public void LoadStageTwoBoss()
    {
        GameManager.setIndex(11);
        SceneManager.LoadScene("SampleScene");
    }



    
}