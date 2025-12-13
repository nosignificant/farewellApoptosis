using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class p53Log : MonoBehaviour
{
    public ChatLog chatLog;
    private bool isPlayerIn = false;
    [SerializeField] private float delay = 2f;

    [Header("Room Settings")]
    public string roomID;
    public Spawner linkedSpawner;

    [Header("Dialogue State")]
    public int chatLogLine = 0;

    private List<string> currentRoomLines;
    private Coroutine dialogueCoroutine;
    private string lastConditionKey = "";
    private List<string> readDialog = new List<string>();
    // 이벤트
    public static event Action<string, string> OnRoomConditionMet;
    void OnEnable()
    {
        if (Player.Instance != null)
            Player.Instance.OnPlayerRoom += SetIsPlayerIn;
    }

    void OnDisable()
    {
        if (Player.Instance != null)
            Player.Instance.OnPlayerRoom -= SetIsPlayerIn;
    }
    void Start()
    {    //방 등록 
        var currentRoom = Util.FindCurrentRoom(this.transform.position);
        if (currentRoom != null) roomID = currentRoom.roomID;
    }

    void Update()
    {
        if (isPlayerIn && Player.Instance.roomID == roomID)
        {
            string newConditionkey = CheckCurrentCondition();

            if (newConditionkey != lastConditionKey && !readDialog.Contains(newConditionkey))
            {
                lastConditionKey = newConditionkey;
                OnRoomConditionMet?.Invoke(roomID, newConditionkey);
                StartDialog(newConditionkey);
            }
        }
        else { StopDialog(); }
    }

    void StartDialog(string conditionKey)
    {
        if (dialogueCoroutine != null) StopDialog();

        currentRoomLines = DialogueData.GetDialogueLines(roomID, conditionKey);
        chatLogLine = 0;

        if (currentRoomLines != null && currentRoomLines.Count > 0)
        {
            dialogueCoroutine = StartCoroutine(Dialog(conditionKey));
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
    IEnumerator Dialog(string key)
    {
        if (!readDialog.Contains(key)) readDialog.Add(lastConditionKey);

        for (int i = chatLogLine; i < currentRoomLines.Count; i++)
        {
            chatLog.Post(currentRoomLines[i]);
            chatLogLine++;

            float t = 0f;
            while (t < delay)
            {
                if (!isPlayerIn) yield break;
                t += Time.deltaTime;
                yield return null;
            }
        }
    }

    string CheckCurrentCondition()
    {
        if (roomID == "tut_00")
        {
            if (Player.circuit) return "hasCircuit";
            if (Player.isPlayerLockOn) return "lockedOn";

        }

        if (roomID == "tut_01")
        {
            if (linkedSpawner != null && linkedSpawner.SpawnerHasCircuit)
                return "repaired";
        }
        if (roomID == "tut_05")
        {
            if (CurrentSentient.Instance != null)
            {
                Debug.Log("current sentient !! " + CurrentSentient.Instance.GetSentientCount(0));
                if (CurrentSentient.Instance.GetSentientCount(0) > 0) return "endTutorial";
            }

        }
        return "startEvent";
    }

    void SetIsPlayerIn(string roomID)
    {
        if (Player.Instance.roomID == roomID)
            isPlayerIn = true;
        else isPlayerIn = false;
        Debug.Log("is player " + roomID + isPlayerIn);
    }


}