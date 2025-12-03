using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [Header("방 설정")]
    public string roomID;
    public Collider roomCollider; // 이 방의 이동 가능한 범위를 나타내는 콜라이더

    [Header("연결된 방 목록")]
    public List<Room> connectedRooms = new List<Room>();

    [Header("방 내부 생물 관리")]
    public List<Creature> activeCreatures = new List<Creature>();
    public List<Spawner> activeSpawners = new List<Spawner>();
    public List<Food> activeFoods = new List<Food>();
    void Awake()
    {
        // 본인 콜라이더 설정
        if (roomCollider == null)
            roomCollider = GetComponent<Collider>();
        // 콜라이더 이름 설정
        if (string.IsNullOrEmpty(roomID))
            roomID = gameObject.name;

        foreach (Room neighbor in connectedRooms)
        {
            if (neighbor != null)
            {
                // 상대방 리스트에 내가 없다면 추가
                if (!neighbor.connectedRooms.Contains(this))
                {
                    neighbor.connectedRooms.Add(this);
                }
            }
        }
    }

    void Start()
    {
        // 💡 핵심: Start에서 등록해야 매니저가 확실히 살아있습니다.
        if (RoomManager.Instance != null)
            RoomManager.Instance.RegisterRoom(this);

        else
        {
            Debug.LogError("RoomManager가 씬에 없습니다!");
        }
    }

    //생명체가 방에 들어올때
    public void OnCreatureEnter(Creature creature)
    {
        if (!activeCreatures.Contains(creature))
        {
            activeCreatures.Add(creature);
        }
    }

    // 생명체가 이 방을 떠날 때 호출
    public void OnCreatureExit(Creature creature)
    {
        if (activeCreatures.Contains(creature))
        {
            activeCreatures.Remove(creature);
            // Debug.Log($"[Room {roomID}] {creature.name} left.");
        }
    }


}