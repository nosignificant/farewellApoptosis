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

    // [수정 1] 방이 바뀌었는지 확인하기 위한 변수 추가
    private string lastRoomID = "";
    private string lastConditionKey = "";

    // [수정 2] 읽은 대화를 "방ID_키값" 형태로 저장하여 중복 방지
    private List<string> readDialog = new List<string>();

    public static event Action<string, string> OnRoomConditionMet;

    void Update()
    {
        if (Player.Instance == null) return;

        string currentPlayerRoom = Player.Instance.roomID;

        if (!string.IsNullOrEmpty(currentPlayerRoom))
        {
            // [수정 3] 방이 바뀌었으면 이전 상태 키를 초기화해서 같은 키("startEvent")가 또 들어와도 인식하게 함
            if (currentPlayerRoom != lastRoomID)
            {
                lastRoomID = currentPlayerRoom;
                lastConditionKey = "";
            }

            string newConditionkey = CheckCurrentCondition(currentPlayerRoom);

            // 읽음 확인용 고유 키 생성 (예: "tut_00_startEvent")
            string uniqueReadCheckKey = currentPlayerRoom + "_" + newConditionkey;

            // [수정 4] 조건 비교 시 readDialog 체크를 고유 키로 변경
            if (newConditionkey != lastConditionKey && !readDialog.Contains(uniqueReadCheckKey))
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
        // [수정 5] 대화를 읽음 처리할 때 "방ID_키값"으로 저장
        string uniqueKey = checkRoomID + "_" + key;
        if (!readDialog.Contains(uniqueKey)) readDialog.Add(uniqueKey);

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
        // ... (기존 코드와 동일) ...
        if (currentRoomName == "tut_04")
        {
            if (CurrentSentient.Instance != null)
            {
                if (CurrentSentient.Instance.GetSentientCount(0) > 0)
                    return "endTutorial";
            }
            return "startEvent";
        }

        if (currentRoomName == "tut_00")
        {
            if (Player.circuit) return "hasCircuit";
            if (Player.isPlayerLockOn) return "lockedOn";
            return "startEvent";
        }

        if (currentRoomName == "tut_01")
        {
            if (RoomManager.Instance != null &&
            RoomManager.Instance.GetRoom(currentRoomName).allSpawners[0].SpawnerHasCircuit)
                return "repaired";
            return "startEvent";
        }

        return "startEvent";
    }
}