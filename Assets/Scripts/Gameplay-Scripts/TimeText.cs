using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TimeText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeText;
    IEnumerator xtraTimeRoutine;
    bool isXtraTimeRoutineRunning;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartXtraTime()
    {        
        if (!isXtraTimeRoutineRunning)
        {
            timeText.text = "XTRA";
            xtraTimeRoutine = XtraTimeRoutine();
            StartCoroutine(xtraTimeRoutine);
        }
    }
    public void EndXtraTime()
    {
        try
        {
            StopCoroutine(xtraTimeRoutine);
            isXtraTimeRoutineRunning = false;
            timeText.text = "GAME\nOVER";
        }
        catch (Exception e)
        {
            Debug.Log("EndXtraTime: Could not stop coroutine. Error: " + e);
        }
        
        
    }
    IEnumerator XtraTimeRoutine()
    {
        isXtraTimeRoutineRunning = true;
        while (isXtraTimeRoutineRunning)
        {
            //timeText.text = "XTRA";
            timeText.color = new Color(1f, 0.5f, 0f);
            yield return new WaitForSeconds(0.5f);
            timeText.color = Color.yellow;
            yield return new WaitForSeconds(0.5f);
        }
        yield break;
    }
    public void EndFirstHalfExtraTime()
    {
        StopCoroutine(xtraTimeRoutine);
        isXtraTimeRoutineRunning = false;
        timeText.color = new Color(1f, 0.5f, 0f);
    }
}
