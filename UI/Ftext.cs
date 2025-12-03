using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Ftext : MonoBehaviour
{

    public TMP_Text fText;  // "now producing: X"
    public static Spawner spawner;      // 스포너 참조

    void Update()
    {
        ControlFtext();
    }
    void ControlFtext()
    {
        if (Player.isEaterSelectOpen)
        {
            if (spawner.SpawnerHasCircuit)
            {
                if (!Player.circuit)
                {
                    fText.text = "F 회로 분리";

                }

            }
            else
            {
                if (Player.circuit)
                    fText.text = "F 회로 연결";
                else fText.text = "연결된 회로 없음";
            }

        }
    }
    public static void SetSpawner(Spawner spawnerObject)
    {
        spawner = spawnerObject;
    }
}