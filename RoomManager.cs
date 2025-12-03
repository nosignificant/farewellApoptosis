using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance; // 싱글톤

    // ID로 방을 빠르게 찾기 위한 딕셔너리
    private Dictionary<string, Room> allRooms = new Dictionary<string, Room>();
    private Dictionary<string, List<Spawner>> allSpawners = new Dictionary<string, List<Spawner>>();

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

    //각 방에 있는 스포너도 스스로를 등록함
    public void RegisterSpawner(string roomID, Spawner spawner)
    {
        // 1. 만약 이 방ID로 된 리스트가 아직 없으면 -> 새로 만듭니다.
        if (!allSpawners.ContainsKey(roomID))
        {
            allSpawners.Add(roomID, new List<Spawner>());
        }

        // 2. 그 방의 리스트에 스포너를 추가합니다.
        // (중복 등록 방지)
        if (!allSpawners[roomID].Contains(spawner))
        {
            allSpawners[roomID].Add(spawner);
            Debug.Log($"Spawner registered to Room: {roomID}");
        }
    }

    // 외부에서 ID로 방 정보 가져오기
    public Room GetRoom(string id)
    {
        if (allRooms.ContainsKey(id)) return allRooms[id];
        Debug.LogWarning($"Room ID '{id}' not found!");
        return null;
    }

    // (옵션) 특정 방에 생물이 몇 마리인지 확인
    public int GetCreatureCountInRoom(string id)
    {
        var room = GetRoom(id);
        return room != null ? room.activeCreatures.Count : 0;
    }

    public List<Spawner> GetSpawnersInRoom(string roomID)
    {
        if (allSpawners.ContainsKey(roomID)) return allSpawners[roomID];
        return null;
    }
    //Spawner은 지금 자기가 스스로 콜라이더 겹치는 거 찾아서 등록하고있는데, 
    //roommanger 전체에서 관리할 수 있게 해야함
}