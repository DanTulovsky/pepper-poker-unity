using System;
using System.Collections;
using Poker;
using QuantumTek.QuantumUI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class Avatar : MonoBehaviour
{
    public Animator anim;
    public QUI_Window speechBubbleWindow;
    public TMP_Text speechBubbleText;
    
    private Game game;
    private ClientInfo clientInfo;

    private static readonly int ShakeHead = Animator.StringToHash("HeadShake");
    private static readonly int Defeated = Animator.StringToHash("Defeated");
    private static readonly int Victory = Animator.StringToHash("Victory");
    private static readonly int Wave = Animator.StringToHash("Waving");

    public IEnumerator Say(string msg)
    {
        Debug.Log("saying hi");
        speechBubbleWindow.SetActive(true);
        speechBubbleText.SetText(msg);

        yield return new WaitForSeconds(TimeSpan.FromSeconds(5).Seconds);
        Debug.Log("exiting");
        speechBubbleWindow.SetActive(false);
    }
    
    public void AnimateShrug(string msg)
    {
        anim.SetTrigger(ShakeHead);
        Debug.Log(msg);
    }

    public void AnimateDefeat(string msg)
    {
        anim.SetTrigger(Defeated);
        Debug.Log(msg);
    }
    
    public void AnimateWave()
    {
        Debug.Log("waving");
        anim.SetTrigger(Wave);
    }
    
    public void AnimateVictory()
    {
        anim.SetTrigger(Victory );
    }
    
    private void Start()
    {
        game = Manager.Instance.game;
        Assert.IsNotNull(game);
        
        clientInfo = Manager.Instance.clientInfo;
        Assert.IsNotNull(clientInfo);

        speechBubbleWindow.SetActive(false);
    }
}
