using UnityEngine;
using TMPro;
using System; // Enum을 쓰기 위해 필요

public class CreatureStateUI : MonoBehaviour
{
    public Creature targetCreature;
    public GameObject statuesRectPrefab;
    public Transform gridParent;

    private PlayerLockOn playerLockOn;

    private string lastKnownState;

    void Start()
    {
        if (Player.Instance != null)
            playerLockOn = Player.Instance.GetComponent<PlayerLockOn>();
        targetCreature = playerLockOn.targetCreature;
        RefreshStateList();
    }

    void Update()
    {
        targetCreature = playerLockOn.targetCreature;

        if (targetCreature == null) return;
        string currentState = targetCreature.statues.ToString();

        if (currentState != lastKnownState)
        {
            lastKnownState = currentState;
            RefreshStateList();
        }
    }

    void RefreshStateList()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        if (targetCreature == null) return;
        Creature.CreatureState currentEnum = targetCreature.GetCurrentState();


        foreach (Creature.CreatureState state in Enum.GetValues(typeof(Creature.CreatureState)))
        {
            if (state != currentEnum)
            {
                GameObject go = Instantiate(statuesRectPrefab, gridParent);

                TMP_Text txt = go.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.text = state.ToString(); // Enum 이름을 글자로 (예: "Eating")
                }
            }
        }
    }
}