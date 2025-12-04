using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance; // 싱글톤

    // ID로 방을 빠르게 찾기 위한 딕셔너리
    private Dictionary<string, Room> allRooms = new Dictionary<string, Room>();
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 각 Room가 Awake될 때 스스로를 등록함
    public void RegisterRoom(Room room)
    {
        if (!allRooms.ContainsKey(room.roomID))
        {
            allRooms.Add(room.roomID, room);
        }
    }

    // 외부에서 ID로 방 정보 가져오기
    public Room GetRoom(string roomID)
    {
        if (allRooms.ContainsKey(roomID)) return allRooms[roomID];
        Debug.LogWarning($"Room roomID '{roomID}' not found!");
        return null;
    }

    // (옵션) 특정 방에 생물이 몇 마리인지 확인
    public int GetCreatureCountInRoom(string roomID)
    {
        var room = GetRoom(roomID);
        return room != null ? room.activeCreatures.Count : 0;
    }

    public List<Spawner> GetSpawnersInRoom(string roomID)
    {
        return GetRoom(roomID).allSpawners;
    }
    //Spawner은 지금 자기가 스스로 콜라이더 겹치는 거 찾아서 등록하고있는데, 
    //roommanger 전체에서 관리할 수 있게 해야함
}