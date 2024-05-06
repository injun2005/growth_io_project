using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XpBar : MonoBehaviour
{
    private int curLv = 0;
    private int curXp = 0;

    [SerializeField]
    private TMP_Text xpText;
    [SerializeField]
    private TMP_Text lvText;
    [SerializeField]
    private Image fillImage;
    private RectTransform rect;

    private void Start()
    {
        rect = (RectTransform)transform;
        fillImage.fillAmount = 0;
    }

    public void OnXpUp(int lv, int xp, int maxXp)
    {
        curXp = xp;
        curLv = lv;

        xpText.SetText($"{curXp}/{maxXp}");
        lvText.SetText($"Level: {curLv}");
        fillImage.fillAmount = xp/(float)maxXp;
    }
   
}
