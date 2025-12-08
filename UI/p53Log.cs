using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class p53Log : MonoBehaviour
{
    public ChatLog chatLog;
    private bool isPlayerIn = false;
    [SerializeField] private float delay = 2f;

    [Header("Room Settings")]
    public string roomID;
    public Spawner linkedSpawner;
    public Door linkedDoor;

    [Header("Dialogue State")]
    public int chatLogLine = 0;
    public bool isDialogueFinished = false;

    private List<string> currentRoomLines;
    private Coroutine dialogueCoroutine;
    private Player currentPlayer;

    // 💡 방금 재생한 상태를 기억하는 변수 (중복 실행 방지용)
    private string lastConditionKey = "";

    void Start()
    {
        var currentRoom = Util.FindCurrentRoom(this.transform.position);
        if (currentRoom != null) roomID = currentRoom.roomID;
    }

    // 💡 1. 실시간 상태 감지 (Update)
    void Update()
    {
        if (isPlayerIn && currentPlayer != null)
        {
            int visitCount = currentPlayer.GetVisitCount(roomID);
            string newConditionKey = CheckCurrentCondition(visitCount);

            if (newConditionKey != lastConditionKey)
            {
                Debug.Log($"상태 변경 감지: {lastConditionKey} -> {newConditionKey}");
                ProcessCondition(newConditionKey);
            }
        }
    }

    // 💡 2. 입장 처리 (OnTriggerEnter)
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlayerIn) return;

        isPlayerIn = true;
        isDialogueFinished = false;
        currentPlayer = other.GetComponent<Player>();

        if (currentPlayer != null)
        {
            lastConditionKey = "";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isPlayerIn == false) return;
        if (!isDialogueFinished && currentRoomLines != null && currentRoomLines.Count > 0)
        {
            Debug.Log("대사 중단됨.");
        }
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        isPlayerIn = false;
        currentPlayer = null;
        lastConditionKey = "";
    }

    // 💡 4. 액션 및 대사 실행 통합 함수
    void ProcessCondition(string conditionKey)
    {
        // (1) 상태 업데이트 (중복 방지)
        lastConditionKey = conditionKey;

        if (roomID == "tut_00" && conditionKey == "hasCircuit")
        {
            if (linkedDoor != null) { linkedDoor.OpenTheDoor(true); }
        }
        if (roomID == "tut_01" && conditionKey == "repaired")
        {
            if (linkedDoor != null) linkedDoor.OpenTheDoor(true);
        }
        if (roomID == "tut_04" && conditionKey == "endTutorial")
        {
            if (linkedDoor != null) linkedDoor.OpenTheDoor(true);
        }


        // (3) 대사 실행 로직
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine); // 기존 대사 끊기

        currentRoomLines = DialogueData.GetDialogueLines(roomID, conditionKey);
        chatLogLine = 0;

        if (currentRoomLines != null && currentRoomLines.Count > 0)
        {
            isDialogueFinished = false;
            dialogueCoroutine = StartCoroutine(Dialog());
        }
    }

    string CheckCurrentCondition(int visited)
    {

        if (roomID == "tut_00" && visited == 1)
        {
            if (Player.circuit) return "hasCircuit";
        }

        if (roomID == "tut_01")
        {
            if (linkedSpawner != null && linkedSpawner.SpawnerHasCircuit)
                return "repaired";
        }
        if (roomID == "tut_04")
        {
            if (CurrentSentient.Instance != null && CurrentSentient.Instance.GetSentientCount(0) > 0)
                return "endTutorial";
        }
        if (visited == 0) return "startEvent";
        return "endEvent";
    }

    IEnumerator Dialog()
    {
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

        if (currentRoomLines.Count == chatLogLine)
        {
            isDialogueFinished = true;
            if (currentPlayer != null)
            {
                currentPlayer.AddVisitRecord(roomID);
                Debug.Log($"{roomID} 대사 완료.{currentPlayer.GetVisitCount(roomID)}");
            }
        }
    }
}