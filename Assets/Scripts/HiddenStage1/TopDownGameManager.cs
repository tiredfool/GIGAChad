using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TopDownGameManager : MonoBehaviour
{
    private Vector3 playerSavedPosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // 플레이어가 충돌하면
        {
            playerSavedPosition = collision.transform.position; // 플레이어 위치 저장
            PlayerPrefs.SetFloat("SavedPosX", playerSavedPosition.x);
            PlayerPrefs.SetFloat("SavedPosY", playerSavedPosition.y);
            PlayerPrefs.SetFloat("SavedPosZ", playerSavedPosition.z);

            // 현재 씬을 비활성화 (씬을 유지한 채로 진행)
            Scene platformerScene = SceneManager.GetActiveScene();
            StartCoroutine(LoadTopDownScene(platformerScene));
        }
    }

    IEnumerator LoadTopDownScene(Scene platformerScene)
    {
        // 현재 씬 비활성화
        foreach (GameObject obj in platformerScene.GetRootGameObjects())
        {
            obj.SetActive(false);
        }

        // 톱다운 씬 추가
        yield return SceneManager.LoadSceneAsync("HiddenStage1", LoadSceneMode.Additive);

        // 톱다운 씬 활성화
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("HiddenStage1"));
    }
}
