using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Etext : MonoBehaviour
{

    public TMP_Text eText;  // "now producing: X"

    void Update()
    {
        ControlEtext();
    }
    void ControlEtext()
    {
        if (Player.isEaterSelectOpen)
        {
            eText.text = "E 결정";
        }
        if (!Player.isEaterSelectOpen)
        {
            eText.text = "E 열기";
        }
    }


}