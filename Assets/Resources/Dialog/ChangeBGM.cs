using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBGM : MonoBehaviour
{
    public string BGM;
    public bool forced = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (forced)
        {
            MainSoundManager.instance.StopBGM();
            MainSoundManager.instance.PlayBGM(BGM);
        }
        else MainSoundManager.instance.ChangeBGM(BGM);

    }
}
