using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SugorokuUI : MonoBehaviour
{
    public Text playerNameText = null;
    public Text diceRollText = null;
    public Text winnerNameText = null;

    public static SugorokuUI instance = null;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDiceRollText(string playerName, int diceNumber)
    {
        playerNameText.text = playerName;
        diceRollText.text = $"{((diceNumber == 0) ? (1 + Random.Range(0, 6)) : diceNumber)}";
    }

    public void SetWinnerName(string winnerName)
    {
        winnerNameText.transform.parent.gameObject.SetActive(true);
        winnerNameText.text = $"{winnerName} win!";
    }
}
