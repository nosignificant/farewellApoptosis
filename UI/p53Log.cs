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

    public int chatLogLine = 0;
    public bool isDialogueFinished = false;
    private List<string> currentRoomLines;
    private Coroutine dialogueCoroutine;
    private Player currentPlayer;

    void Start()
    {
        //p53 대사 위치는 바뀌지 않음, 플레이어 스탯 가져와서 플레이어 스탯에 따라 다른 대사를 하게끔 함 
        roomID = Util.FindCurrentRoom(this.transform.position).roomID;
        linkedSpawner = Util.FindCurrentRoomSpawner(roomID);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlayerIn) return;

        isPlayerIn = true;
        isDialogueFinished = false;

        currentPlayer = other.GetComponent<Player>();

        if (currentPlayer != null)
        {
            // 💡 수정 2: 방문 횟수(int) 대신 현재 상태 조건(string)을 가져옵니다.
            string conditionKey = CheckCurrentCondition();

            Debug.Log($"Room: {roomID}, Condition: {conditionKey}");

            // 💡 수정 3: string 키를 전달하여 대사를 로드합니다.
            currentRoomLines = DialogueData.GetDialogueLines(roomID, conditionKey);

            chatLogLine = 0;

            if (currentRoomLines != null && currentRoomLines.Count > 0)
            {
                dialogueCoroutine = StartCoroutine(Dialog());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isDialogueFinished && currentRoomLines != null && currentRoomLines.Count > 0)
        {
            chatLog.Post("안내를 끝까지 듣고 이동하시길 바랍니다.");
            Debug.Log("대사 중단됨. 방문 횟수 증가 안 함.");
        }

        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        isPlayerIn = false;
        currentPlayer = null;
    }

    // 💡 상태 체크 함수
    string CheckCurrentCondition()
    {
        if (roomID == "tut_01")
        {
            // linkedSpawner가 할당되어 있다면 상태 확인
            if (linkedSpawner != null && linkedSpawner.SpawnerHasCircuit)
            {
                return "Repaired";
            }

            // Player의 static 변수 접근
            if (Player.circuit)
            {
                return "HasCircuit";
            }
        }

        return "Default";
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
                Debug.Log($"{roomID} 대사 완료. 방문 횟수 증가.");
            }
        }
    }
}