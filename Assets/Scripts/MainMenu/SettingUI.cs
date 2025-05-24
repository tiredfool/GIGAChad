using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public Image setting;
    public bool isActivated;

    // Start is called before the first frame update
    void Start()
    {
        isActivated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (isActivated == false)
            {
                setting.gameObject.SetActive(true);
                isActivated = true;
                Time.timeScale = 0f;
            }
            else
            {
                setting.gameObject.SetActive(false);
                isActivated = false;
                Time.timeScale = 1f;
            }
        }

        
    }
    public void togleActive()
    {
        if (isActivated == false)
        {
            setting.gameObject.SetActive(true);
            isActivated = true;
            Time.timeScale = 0f;
        }
        else
        {
            setting.gameObject.SetActive(false);
            isActivated = false;
            Time.timeScale = 1f;
        }
    }

}
