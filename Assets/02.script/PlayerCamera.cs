using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private GhostScript player;

    void Update()
    {
        transform.position = player.GetCameraPos();
    }
}
