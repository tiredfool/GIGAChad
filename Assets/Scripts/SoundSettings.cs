using UnityEngine;

public class SoundSettings : MonoBehaviour
{
   
    public static SoundSettings Instance { get; private set; }
    public float MasterBGMVolume  = 1f;
    public float MasterSFXVolume  = 1f;

    void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

  
   
}