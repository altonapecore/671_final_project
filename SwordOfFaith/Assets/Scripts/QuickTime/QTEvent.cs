using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New QuickTimeEvent", menuName = "QuickTimeEvents")]
public class QTEvent : ScriptableObject
{
    [Header("QuickTime Settings")]
    public QTType quickTimeType;
    [ConditionalHide("isRhythm", true, true)]
    public float DELAY_BEFORE_QT_FAIL;
    [Space(5)]
    public List<char> quickTimePossibleKeys;

    [Header("QuickTimeEvent Settings")]
    [Space(10)]
    //KeyCombo
    [ConditionalHide("isKeyCombo", true)]
    public int MIN_TOTAL_INPUTS;
    [ConditionalHide("isKeyCombo", true)]
    public int MAX_TOTAL_INPUTS;

    //Rhythm
    [ConditionalHide("isRhythm", true)]
    public float INPUT_KEY_DELAY;
    [ConditionalHide("isRhythm", true)]
    public float MAX_DELAY_BEFORE_FAIL;

    //Mash
    [ConditionalHide("isMash", true)]
    public int MIN_POSSIBLE_NEEDED_KEYHITS;
    [ConditionalHide("isMash", true)]
    public int MAX_POSSIBLE_NEEDED_KEYHITS;

    [HideInInspector]
    public bool isKeyCombo, isRhythm, isMash;
    public enum QTType { KeyCombo, Rhythm, Mash }

    /// <summary>
    /// Conditional Hide Inspector Tools
    /// </summary>
    private void OnValidate()
    {
        if(quickTimeType == QTType.KeyCombo && (!isKeyCombo || (isKeyCombo && isRhythm)))
        {
            isKeyCombo = true;
            isRhythm = false;
            isMash = false;
        }
        else if (quickTimeType == QTType.Rhythm && !isRhythm)
        {
            if (!isKeyCombo)
            {
                isKeyCombo = true;
            }
            isRhythm = true;
            isMash = false;
        }
        else if (quickTimeType == QTType.Mash && !isMash)
        {
            isKeyCombo = false;
            isRhythm = false;
            isMash = true;
        }
    }
}
