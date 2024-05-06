using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField]
    private FoodStat stat;

    private int level;
    private int xp;
    private Renderer renderer;
    private bool isChanged = false;
    public int XP
    {
        get { return (int)stat.xp; }
    }
    private void Start()
    {
        level = stat.level;
        xp = stat.xp;
        renderer = GetComponent<Renderer>();
    }

    public bool CheckEating(int pLevel)
    {
        if (pLevel >= level && !isChanged)
        {
            isChanged = true;
            OnAte();
            return true;
        }
        return false;
    }
    private void OnAte()
    {
        renderer.material.color = Color.black;
    }
}
