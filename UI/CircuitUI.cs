using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircuitUI : MonoBehaviour
{

    public TMP_Text circuitText;  // "now producing: X"

    void Update()
    {
        ControlCircuittext();
    }
    void ControlCircuittext()
    {
        if (Player.isEaterSelectOpen)
        {
            circuitText.text = "circuit O";
        }
        if (!Player.isEaterSelectOpen)
        {
            circuitText.text = "circuit X";
        }
    }


}