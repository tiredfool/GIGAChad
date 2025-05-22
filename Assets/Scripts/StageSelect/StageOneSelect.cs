using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageOneSelect : MonoBehaviour
{
   

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadStageOneBoss()
    {
        
        SceneManager.LoadScene("SampleScene");
    }

    

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var gameManager = GameObject.Find("GameManager");
        var script = gameManager.GetComponent<GameManager>();
        script.stageIndex = 7;
    }

    
}