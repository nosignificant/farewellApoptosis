using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircuitUI : MonoBehaviour
{
    public TMP_Text circuit;  // "now producing: X"

    void start()
    {
        circuit = this.gameObject.GetComponent<TMP_Text>();
        circuit.text = "회로 소지 : X";
    }

    void Update()
    {
        ControlCircuittext();
    }
    void ControlCircuittext()
    {
        if (Player.circuit)
        {
            circuit.text = "회로 소지 : O";
        }
        if (!Player.circuit)
        {
            circuit.text = "회로 소지 : X";
        }
    }


}