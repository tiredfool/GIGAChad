using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameExit : MonoBehaviour
{
    public void Exit()
    {
        Debug.Log("게임 종료");
        Application.Quit();

    }
}
