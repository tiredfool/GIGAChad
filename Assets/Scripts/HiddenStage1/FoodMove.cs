using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMove : MonoBehaviour
{
    public Transform target;

    private AIDestinationSetter destinationSetter;

    void Start()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();

        if (destinationSetter != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerHidden");
            if (playerObj != null)
            {
                target = playerObj.transform;
                destinationSetter.target = target;
                Debug.Log("Target set to: " + target.name);
            }
            else
            {
                Debug.LogWarning("No GameObject with tag 'PlayerHidden' found.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
        else if (other.CompareTag("PlayerHidden"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.hp--;
            }

            Destroy(gameObject); // ºÎµúÈù ÈÄ »ç¶óÁö°Ô
        }
    }
}