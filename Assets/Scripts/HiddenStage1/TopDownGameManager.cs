using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TopDownGameManager : MonoBehaviour
{
    private Vector3 playerSavedPosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // �÷��̾ �浹�ϸ�
        {
            playerSavedPosition = collision.transform.position; // �÷��̾� ��ġ ����
            PlayerPrefs.SetFloat("SavedPosX", playerSavedPosition.x);
            PlayerPrefs.SetFloat("SavedPosY", playerSavedPosition.y);
            PlayerPrefs.SetFloat("SavedPosZ", playerSavedPosition.z);

            // ���� ���� ��Ȱ��ȭ (���� ������ ä�� ����)
            Scene platformerScene = SceneManager.GetActiveScene();
            StartCoroutine(LoadTopDownScene(platformerScene));
        }
    }

    IEnumerator LoadTopDownScene(Scene platformerScene)
    {
        // ���� �� ��Ȱ��ȭ
        foreach (GameObject obj in platformerScene.GetRootGameObjects())
        {
            obj.SetActive(false);
        }

        // ��ٿ� �� �߰�
        yield return SceneManager.LoadSceneAsync("HiddenStage1", LoadSceneMode.Additive);

        // ��ٿ� �� Ȱ��ȭ
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("HiddenStage1"));
    }
}
