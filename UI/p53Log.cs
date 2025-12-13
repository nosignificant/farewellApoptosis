using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class p53Log : MonoBehaviour
{
    public ChatLog chatLog;
    [SerializeField] private float delay = 2f;

    [Header("Dialogue State")]
    public int chatLogLine = 0;

    private List<string> currentRoomLines;
    private Coroutine dialogueCoroutine;
    private string lastConditionKey = "";
    private List<string> readDialog = new List<string>();

    public static event Action<string, string> OnRoomConditionMet;

    void Update()
    {
        if (Player.Instance == null) return;

        string currentPlayerRoom = Player.Instance.roomID;

        if (!string.IsNullOrEmpty(currentPlayerRoom))
        {
            string newConditionkey = CheckCurrentCondition(currentPlayerRoom);

            if (newConditionkey != lastConditionKey && !readDialog.Contains(newConditionkey))
            {
                lastConditionKey = newConditionkey;
                OnRoomConditionMet?.Invoke(currentPlayerRoom, newConditionkey);

                StartDialog(newConditionkey, currentPlayerRoom);
            }
        }
        else
        {
            StopDialog();
        }
    }

    // 인자 추가: 대사를 가져올 방 ID
    void StartDialog(string conditionKey, string targetRoomID)
    {
        if (dialogueCoroutine != null) StopDialog();

        currentRoomLines = DialogueData.GetDialogueLines(targetRoomID, conditionKey);
        chatLogLine = 0;

        if (currentRoomLines != null && currentRoomLines.Count > 0)
        {
            dialogueCoroutine = StartCoroutine(Dialog(conditionKey, targetRoomID));
        }
    }

    void StopDialog()
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }
    }

    IEnumerator Dialog(string key, string checkRoomID)
    {
        if (!readDialog.Contains(key)) readDialog.Add(lastConditionKey);

        for (int i = chatLogLine; i < currentRoomLines.Count; i++)
        {
            chatLog.Post(currentRoomLines[i]);
            chatLogLine++;

            float t = 0f;
            while (t < delay)
            {

                if (Player.Instance.roomID != checkRoomID) yield break;

                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    string CheckCurrentCondition(string currentRoomName)
    {
        if (currentRoomName == "tut_04")
        {
            if (CurrentSentient.Instance != null)
            {
                if (CurrentSentient.Instance.GetSentientCount(0) > 0)
                    return "endTutorial";
            }
        }

        if (currentRoomName == "tut_00")
        {
            if (Player.circuit) return "hasCircuit";
            if (Player.isPlayerLockOn) return "lockedOn";
        }

        if (currentRoomName == "tut_01")
        {
            if (RoomManager.Instance != null &&
            RoomManager.Instance.GetRoom(currentRoomName).allSpawners[0].SpawnerHasCircuit)
                return "repaired";
        }

        return "startEvent";
    }
}