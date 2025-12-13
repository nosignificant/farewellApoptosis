using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Spawner : MonoBehaviour, IInteractable
{
    //스포너들
    public GameObject[] eaterPrefabs;
    public int spawnPrefabIndex = 0;
    public int spawnCount = 5;
    public float spawnRadius = 5f;

    public List<GameObject> spawnedEaters = new List<GameObject>();

    // UI setting 
    public bool isPlayerIn = false;

    public GameObject eaterSelector;

    // circuit
    [SerializeField]
    private bool _spawnerHasCircuit = true;

    // 2. 외부에서 접근할 '프로퍼티' (public)
    public bool SpawnerHasCircuit
    {
        get { return _spawnerHasCircuit; } // 값을 읽을 때
        set
        {
            // 3. 값이 바뀔 때만 실행하는 로직
            if (_spawnerHasCircuit != value)
            {
                _spawnerHasCircuit = value; // 값을 업데이트

                // 값이 변경되었을 때 실행할 함수 호출
                OnCircuitStateChanged();
            }
        }
    }
    //Room setting 
    public Room currentRoom;

    void Start()
    {
        currentRoom = Util.FindCurrentRoom(transform.position);
        if (SpawnerHasCircuit) AutoSpawn();
        if (currentRoom != null)
            currentRoom.RegisterSpawner(this);
    }

    void Update()
    {
        if (SpawnerHasCircuit && spawnedEaters.Count == 0)
        {
            AutoSpawn();
        }
        // 나중에 하세요 if (!SpawnerHasCircuit && spawnedEaters.Count != 0) StartCoroutine(DestroyAll());
    }
    public void AutoSpawn()
    {
        // 길이 0 이면 그냥 끝
        if (eaterPrefabs.Length == 0) return;
        // 프리팹 할당
        GameObject prefab = eaterPrefabs[spawnPrefabIndex];
        //랜덤 위치에 프리팹 instantiate 
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = GetRandomSpawnPosition();
            var go = Instantiate(prefab, pos, Quaternion.identity);

            Eater eaterComponent = go.GetComponent<Eater>();
            if (eaterComponent != null && currentRoom.roomCollider != null)
            {
                eaterComponent.currentRoom = this.currentRoom;
            }
            else if (currentRoom.roomCollider == null)
            {
                Debug.LogWarning(currentRoom.roomID + "Wander Boundary 콜라이더가 스포너에 할당되지 않았습니다! Eater는 영역 제한 없이 배회합니다.");
            }
            // --------------------------------------------------------

            spawnedEaters.Add(go);
        }
    }

    //변수 랜덤 위치 
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 offset = Random.insideUnitSphere * spawnRadius;
        offset.y = 0f;
        return transform.position + offset;
    }
    // 교체
    IEnumerator ReplaceRoutine(int prefabIndex)
    {
        // 1. 기존 것들이 완전히 사라질 때까지 기다림
        yield return StartCoroutine(DestroyAll());

        // 2. 프리팹 인덱스 업데이트
        spawnPrefabIndex = prefabIndex;

        // 3. 스폰 시작 (DestroyAll이 끝난 후 List는 비어있음)
        AutoSpawn();
    }

    public void ReplaceAllWith(int prefabIndex)
    {
        // 💡 수정: Coroutine을 호출하여 순서를 보장합니다.
        StartCoroutine(ReplaceRoutine(prefabIndex));
    }
    //파괴
    private IEnumerator DestroyAll()
    {
        foreach (var eater in spawnedEaters)
        {
            if (eater != null)
                Destroy(eater);
            yield return new WaitForSeconds(0.1f);
        }
        spawnedEaters.Clear();
    }

    void OnCircuitStateChanged()
    {
        if (_spawnerHasCircuit == true)
        {
            Debug.Log("Circuit ON: 스폰을 시작합니다.");
            AutoSpawn(); // 예: 회로가 생기면 스폰 시작
        }
        else
        {
            Debug.Log("Circuit OFF: 모두 파괴합니다.");
            StartCoroutine(DestroyAll()); // 예: 회로가 없어지면 파괴
        }
    }

    // 플레이어와 상호작용 가능한 함수를 물체쪽에, 플레이어는 플레이어의 기능만 
    // 스포너의 일은 스포너가 알아서 
    public bool Interact(Player player)
    {
        // 1. 스포너에 회로가 있고, 플레이어는 없을 때
        if (SpawnerHasCircuit && !Player.circuit)
        {
            SpawnerHasCircuit = false;
            Player.circuit = true;
            Debug.Log("Spawner: 회로를 플레이어에게 줬습니다.");
            return true;
        }
        // 2. 스포너에 회로가 없고, 플레이어가 있을 때 -> 회로 받기
        else if (!SpawnerHasCircuit && Player.circuit)
        {
            SpawnerHasCircuit = true;
            Player.circuit = false;
            Debug.Log("Spawner: 플레이어로부터 회로를 받았습니다.");
            return true;
        }

        return false; // 아무 일도 안 일어남
    }
}
