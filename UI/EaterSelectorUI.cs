using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EaterSelectorUI : MonoBehaviour
{
    public Spawner spawner;      // 스포너 참조
    public GameObject eaterSelector;  // 패널
    public GameObject[] cursors;      // 커서들 (0~3)
    public TMP_Text nowPrefabText;    // "now producing: X"
    public int selectedIndex = 0; // EaterSelectorUI 안에 필드로 두기

    void OnEnable()
    {
        InitializeSelector();
    }
    void Update()
    {
        UpdatePrefabNum();
        ConfirmWithEnter();
        if (Input.GetKeyDown(KeyCode.A)) Debug.Log("keydown A from eaterSelector");
        else if (Input.GetKeyDown(KeyCode.D)) Debug.Log("keydown D from eaterSelector");
    }
    void UpdatePrefabNum()
    {
        if (spawner == null) return;
        if (!spawner.isPlayerIn) return;
        if (!spawner._spawnerHasCircuit) return;
        int delta = 0;

        if (Input.GetKeyDown(KeyCode.A)) delta = -1;
        else if (Input.GetKeyDown(KeyCode.D)) delta = 1;

        if (delta == 0) return;

        int max = spawner.eaterPrefabs.Length;
        if (max == 0) return;

        selectedIndex = (selectedIndex + delta + max) % max;

        int humanIndex = selectedIndex + 1;
        if (nowPrefabText != null)
            nowPrefabText.text = $"now producing: {humanIndex}";

        SetCursor(selectedIndex);
    }

    public void InitializeSelector()
    {
        if (spawner == null) return;

        // 1. 스포너의 현재 인덱스를 selectedIndex에 반영합니다.
        selectedIndex = spawner.spawnPrefabIndex;

        // 2. 해당 인덱스로 UI를 즉시 업데이트합니다.
        SetCursor(selectedIndex);

        // 3. 텍스트도 업데이트합니다.
        int humanIndex = selectedIndex + 1;
        if (nowPrefabText != null)
            nowPrefabText.text = $"now producing: {humanIndex}";
    }

    void SetCursor(int index)
    {
        for (int i = 0; i < cursors.Length; i++)
        {
            if (cursors[i] != null)
                cursors[i].SetActive(i == index);
        }
    }
    void ConfirmWithEnter()
    {
        if (!spawner.isPlayerIn) return;
        if (spawner == null) return;

        if (Input.GetKeyDown(KeyCode.E) && Player.isSelectingEater)
        {
            spawner.ReplaceAllWith(selectedIndex);
        }
    }

    public void SetSpawner(Spawner spawnerObject)
    {
        this.spawner = spawnerObject;
    }
    public void cleanSpawner()
    {
        this.spawner = null;
    }
}