using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageTwoSelect : MonoBehaviour
{


    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadStageTwoBoss()
    {

        SceneManager.LoadScene("SampleScene");
    }



    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // var gameManager = GameObject.Find("GameManager");
        // var script = gameManager.GetComponent<GameManager>();
        GameManager.setIndex(14);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}