using System;
using System.Collections;
using System.Collections.Generic;
using QuantumTek.QuantumUI;
using TMPro;
using UnityEngine;

public class TablePosition : MonoBehaviour
{

    public GameObject playerName;
    public TMP_Text lastActionText;
    public TMP_Text stackText;
    public QUI_Bar radialBar;
    public GameObject cards;
    
    public ParticleSystem pSystem;
    public TMP_Text nameText;
    
    // Start is called before the first frame update
    public void Awake()
    {
        playerName = gameObject.transform.Find("Name").gameObject;
        lastActionText = gameObject.transform.Find("LastAction").gameObject.GetComponent<TMP_Text>();
        stackText = gameObject.transform.Find("Stack").gameObject.GetComponent<TMP_Text>();
        radialBar = gameObject.transform.Find("Radial Bar").gameObject.GetComponent<QUI_Bar>();
        cards = gameObject.transform.Find("Cards").gameObject;
        
        nameText = playerName.GetComponent<TMP_Text>();
        pSystem = playerName.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public IEnumerator PlayWinnerParticles(TimeSpan delay)
    {
        yield return new WaitForSeconds(delay.Seconds);
        pSystem.Play(true);
    }
    
    public void StopWinnerParticles()
    {
        pSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
