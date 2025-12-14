using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EaterSelectorUI : MonoBehaviour
{
    public Spawner spawner;
    public GameObject eaterSelector;
    public TMP_Text nowPrefabText;
    public TMP_Text indexText;

    public int selectedIndex = 0;

    private int currentProducingIndex = 0;

    public GameObject[] models;

    void OnEnable()
    {
        InitializeSelector();
    }

    void OnDisable()
    {
        foreach (GameObject model in models)
            if (model != null) model.SetActive(false);
    }

    void Update()
    {
        if (spawner == null) return;
        if (!spawner.isPlayerIn || !spawner.SpawnerHasCircuit) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ChangeSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.E) && Player.isSelectingEater)
        {
            ConfirmSelection();
        }
    }

    void ChangeSelection(int direction)
    {
        int max = models.Length;
        if (max == 0) return;

        selectedIndex = (selectedIndex + direction + max) % max;
        indexText.text = selectedIndex.ToString();

        UpdateVisuals();
    }

    public void InitializeSelector()
    {
        if (spawner == null) return;

        selectedIndex = spawner.spawnPrefabIndex;

        if (models.Length > 0)
            selectedIndex = Mathf.Clamp(selectedIndex, 0, models.Length - 1);

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // 1. 내가 선택 중인 번호 (Index Text)
        if (indexText != null)
            indexText.text = selectedIndex.ToString();

        for (int i = 0; i < models.Length; i++)
        {
            if (models[i] != null)
                models[i].SetActive(i == selectedIndex);
        }

        // 3. 현재 생산 중인 정보 텍스트
        if (nowPrefabText != null)
        {
            if (!spawner.SpawnerHasCircuit)
                nowPrefabText.text = "No Circuit";
            else
                nowPrefabText.text = $"Current: {currentProducingIndex}";
        }
    }

    void ConfirmSelection()
    {
        spawner.ReplaceAllWith(selectedIndex);
        currentProducingIndex = selectedIndex;
        UpdateVisuals();
    }

    public void SetSpawner(Spawner spawnerObject)
    {
        this.spawner = spawnerObject;
        InitializeSelector();
    }

    public void cleanSpawner()
    {
        this.spawner = null;
    }
}