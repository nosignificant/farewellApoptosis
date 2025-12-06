using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class CurrentSentient : MonoBehaviour
{
    public static CurrentSentient Instance;
    private Dictionary<int, int> sentientData = new Dictionary<int, int>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void UpdateSentient(int creatureID, int amount)
    {
        if (!sentientData.ContainsKey(creatureID))
        {
            sentientData.Add(creatureID, 0);
        }
        // 1을 받아서 더해야 정상적으로 1씩 늘어납니다.
        sentientData[creatureID] += amount;
    }

    public int GetSentientCount(int creatureID)
    {
        if (sentientData.ContainsKey(creatureID)) return sentientData[creatureID];
        return 0;
    }
}