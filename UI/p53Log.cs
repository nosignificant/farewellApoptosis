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
    public Spawner linkedSpanwer;
    public Door linkedDoor;
    public int chatLogLine = 0;
    public bool isDialogueFinished = false;
    private List<string> currentRoomLines;
    private Coroutine dialogueCoroutine;
    private Player currentPlayer;

    void Start()
    {
        //p53 대사 위치는 바뀌지 않음, 플레이어 스탯 가져와서 플레이어 스탯에 따라 다른 대사를 하게끔 함 
        roomID = Util.FindCurrentRoom(this.transform.position).roomID;
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
            int playerVisited = currentPlayer.GetVisitCount(roomID);
            string conditionKey = CheckCurrentCondition(playerVisited);

            Debug.Log($"Room: {roomID}, playerCircuit:{Player.circuit}, Condition: {conditionKey}, visted Count: {playerVisited}");

            // 💡 수정 3: string 키를 전달하여 대사를 로드합니다.
            currentRoomLines = DialogueData.GetDialogueLines(roomID, conditionKey);

            chatLogLine = 0;

            if (currentRoomLines != null && currentRoomLines.Count > 0)
            {//코루틴 끝나면 방문 횟수 증가 
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
    string CheckCurrentCondition(int visited)
    {
        if (roomID == "tut_00")
            if (Player.circuit)
            {
                linkedDoor.OpenTheDoor(true);
                return "hasCircuit";
            }
        if (roomID == "tut_01")
        {// 수리 완료는 연결된 스포너에 회로가 있을 때
            if (linkedSpanwer != null && linkedSpanwer.SpawnerHasCircuit)
                return "repaired";
            //플레이어가 회로를 가지고 있을 때
            if (Player.circuit && !linkedSpanwer.SpawnerHasCircuit)
                return "hasCircuit";
        }
        if (visited == 0)
            return "startEvent";
        //방문 횟수가 0이 아닐때
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
                Debug.Log($"{roomID} 대사 완료. 방문 횟수 증가.");
            }
        }
    }
}