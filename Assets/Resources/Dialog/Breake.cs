using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breake : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void BROKE() {
        Destroy(gameObject);
    }
    public void ON()
    {
        this.gameObject.SetActive(true);
    }
    public void resume()
    {
        Time.timeScale = 1f;
    }
    public void End2()
    {
        DialogueManager.instance.StartDialogueByIdRange("E-2s","E-2e");
      
    }
}
