using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class sentientUI : MonoBehaviour
{
    public int index;
    public TMP_Text amount;  // "now producing: X"

    void Start()
    {
        amount = this.gameObject.GetComponent<TMP_Text>();
        amount.text = "0";
    }

    void Update()
    {
        if (CurrentSentient.Instance != null)
            amount.text = CurrentSentient.Instance.GetSentientCount(index).ToString();

    }
}