using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public GameObject platformPrefab;
    public GameObject blackWaves;
    public GameObject mainCamera;
    public GameObject bossCamera;
    public Transform player;

    public int poolSize = 10;
    public float verticalSpacing = 3f;
    public float minX = 270f;
    public float maxX = 275f;

    private List<GameObject> platformPool = new List<GameObject>();
    private float highestY;

    void Start()
    {
        // 초기 발판 생성
        for (int i = 0; i < poolSize; i++)
        {
            Vector3 pos = new Vector3(Random.Range(minX, maxX), i * verticalSpacing, 0f);
            GameObject plat = Instantiate(platformPrefab, pos, Quaternion.identity);
            platformPool.Add(plat);
            highestY = Mathf.Max(highestY, pos.y);
        }
    }

    void Update()
    {
        foreach (GameObject plat in platformPool)
        {
            if (plat.transform.position.y + 10f < player.position.y)
            {
                // 발판을 위로 재배치
                Vector3 newPos = new Vector3(Random.Range(minX, maxX), highestY + verticalSpacing, 0f);
                plat.transform.position = newPos;
                highestY = newPos.y;
            }
        }
    }

    public void ActivateBossStage()
    {
        if (mainCamera != null) mainCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(true);
        if (blackWaves != null)
        {
            StartCoroutine(RaiseBlackWaves());
        }
    }

    private System.Collections.IEnumerator RaiseBlackWaves()
    {
        float duration = 7f;
        float elapsed = 0f;
        Vector3 startPos = blackWaves.transform.position;
        Vector3 targetPos = startPos + new Vector3(0f, 20f, 0f);

        while (elapsed < duration)
        {
            blackWaves.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        blackWaves.transform.position = targetPos;
    }
}
