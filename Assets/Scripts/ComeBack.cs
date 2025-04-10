using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComeBack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float savedX = PlayerPrefs.GetFloat("SavedPosX", transform.position.x);
        float savedY = PlayerPrefs.GetFloat("SavedPosY", transform.position.y);
        float savedZ = PlayerPrefs.GetFloat("SavedPosZ", transform.position.z);

        transform.position = new Vector3(savedX, savedY, savedZ);
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
