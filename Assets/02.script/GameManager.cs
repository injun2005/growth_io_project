using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager instacne;
    public static GameManager Instance { get { return instacne; } }

    public GhostScript player;
    [SerializeField]
    private XpBar xpBar;
    public UnityEvent onUpXp;

    private void Start()
    {
        instacne = this;

        player = FindObjectOfType<GhostScript>();
        onUpXp.AddListener(UpdateLevelUI);
    }

    private void UpdateLevelUI()
    {
        xpBar.OnXpUp(player.Level, player.XP, player.NeedXp);
    }
}
