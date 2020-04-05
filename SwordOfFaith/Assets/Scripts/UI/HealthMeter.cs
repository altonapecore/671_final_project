using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthMeter : MonoBehaviour
{
    [SerializeField] private Text percentText;
    [SerializeField] private Image percentImage;

    public int MAXHEALTH = 300;

    private PlayerControl[] playerControls;
    private float p;

    public static float Health{get; set;}

    private void Awake()
    {
        Health = MAXHEALTH;
        p = Health / MAXHEALTH;

        SetPercentage(p);
    }

    private void Update()
    {
        p = Mathf.Lerp(p, Health, 0.1f);
        SetPercentage(p / MAXHEALTH);
    }

    /// <summary>
    /// Set the UI values to match percentage
    /// </summary>
    /// <param name="percent">percentage of max health (0, 1)</param>
    public void SetPercentage(float percent)
    {
        percentText.text = $"{Mathf.RoundToInt(percent * 100)}%";
        percentImage.fillAmount = percent;
    }
}
