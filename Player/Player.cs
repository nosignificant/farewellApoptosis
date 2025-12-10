using UnityEngine;
using System.Collections.Generic;


public class Player : MonoBehaviour
{
    // UI 
    public GameObject pressE;
    public GameObject pressF;
    public Spawner Spawner;
    public EaterSelectorUI eaterSelector;
    public static bool isEaterSelectOpen = false;
    public static bool circuit = false;

    // E button options
    public static bool isSelectingEater = false;

    //where is player
    public string roomID;
    private Dictionary<string, int> roomMemories = new Dictionary<string, int>();

    //player status
    public string nowPlayerDoing = null;

    void Update()
    {
        PressE();
        PressF();
        PressEsc();
        SyncUI();
    }

    void OnTriggerEnter(Collider other)
    {
        //스포너에 들어오면 상호작용 e 켜지고 누를 수 있음 
        if (other.CompareTag("Spawner"))
        {
            Spawner = other.GetComponent<Spawner>();
            PressEOnOff(true);

        }
        if (other.CompareTag("Room"))
        {
            roomID = other.name;
        }
    }

    void OnTriggerExit(Collider other)
    {
        //스포너 나가면 상호작용 e 꺼지고 누를 수 e 없어져야함 
        if (other.CompareTag("Spawner"))
        {
            Spawner = other.GetComponent<Spawner>();
            PressEOnOff(false);

            //나갔으니까 플레이어가 상호작용하는 스포너 비워야함 
            Spawner = null;

        }
    }

    void PressE()
    { //플레이어가 e를 누를 수 있는 상태에서 e를 누르면 
        if (Input.GetKeyDown(KeyCode.E))
        { //플레이어가 반경내에 있는 상태의 스포너는 eaterSelector에서 조작? 
            if (Spawner != null && Spawner.isPlayerIn)
            {
                isEaterSelectOpen = true;
                PlayerControl.setPlayerMove(false);
                EaterSelectorUIOnOff(isEaterSelectOpen);
                //eaterselector UI에 플레이어가 상호작용중인 스포너 설정
                eaterSelector.SetSpawner(Spawner);
            }
        }
        if (Spawner != null)
            if (isEaterSelectOpen && !Spawner.SpawnerHasCircuit) PressEOnOff(false);
            else PressEOnOff(true);

    }

    void PressF()
    {
        if (!isEaterSelectOpen || !Input.GetKeyDown(KeyCode.F)) return;

        if (Spawner != null)
        {
            IInteractable interactable = Spawner.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact(this); // "나(Player)랑 상호작용 하자!"
            }
        }
    }

    //esc 누르면 모든 ui끄기
    void PressEsc()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isEaterSelectOpen)
            {
                EaterSelectorUIOnOff(false);
                PlayerControl.setPlayerMove(true);
                PressEOnOff(true);
            }

        }
    }
    void SyncUI()
    {// 플레이어가 반경을 나가면 e, enter, selector 다 꺼야하고 
        if (Spawner == null)
        {
            EaterSelectorUIOnOff(false);
            eaterSelector.cleanSpawner();
            return;
        }
    }

    void EaterSelectorUIOnOff(bool OnOff)
    {
        isEaterSelectOpen = OnOff;
        isSelectingEater = OnOff;

        if (eaterSelector != null)
            eaterSelector.gameObject.SetActive(OnOff);
        if (pressF != null)
        {
            Ftext.SetSpawner(Spawner);
            pressF.SetActive(OnOff);

        }
    }

    void PressEOnOff(bool onOff)
    {
        if (Spawner)
            Spawner.isPlayerIn = onOff;
        pressE.SetActive(onOff);
    }

    public int GetVisitCount(string roomID)
    {
        if (roomMemories.ContainsKey(roomID))
            return roomMemories[roomID];
        return 0; // 처음이면 0
    }

    public void AddVisitRecord(string roomID)
    {
        if (roomMemories.ContainsKey(roomID))
            roomMemories[roomID]++;
        else
            roomMemories[roomID] = 1;

        Debug.Log($"[Player Memory] Room {roomID} visited {roomMemories[roomID]} times.");
    }

}