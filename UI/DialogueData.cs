using System.Collections.Generic;

public static class DialogueData
{

    public static readonly Dictionary<string, Dictionary<string, List<string>>> DialogueMapByState =
        new Dictionary<string, Dictionary<string, List<string>>>()
    {

        {"tut_00", new Dictionary<string, List<string>>
            {
                // 기본 상태 (아무 조건 없을 때)
                {"Default", new List<string>
                    {
                        "안녕하세요. 저는 p53입니다.",
                        "시설 수리를 위한 교육을 시작합니다.",
                    }
                },
            }
        },

        // ----------------------------------------------------
        // Room: tut_01 (생산기 방)
        // ----------------------------------------------------
        {"tut_01", new Dictionary<string, List<string>>
            {
                // 1. 처음 도착했을 때 (기본)
                {"Default", new List<string>
                    {
                        "이곳에 보이는 것은 생산 나무입니다.",
                        "생물들은 이 나무에서 자라는 정보를 먹고 활동합니다.",
                        "이곳의 생산기를 조사하십시오."
                    }
                },
                // 2. 플레이어가 회로를 가지고 있을 때
                {"HasCircuit", new List<string>
                    {
                        "회로를 가지고 계시군요!",
                        "그 회로를 생산기에 가까이 가져가 F키를 눌러 장착하십시오.",
                        "생산기가 작동하면 생물이 태어날 것입니다."
                    }
                },
                // 3. 이미 수리가 끝났을 때
                {"Repaired", new List<string>
                    {
                        "생산기가 정상 작동 중입니다.",
                        "다음 구역으로 이동하십시오."
                    }
                }
            }
        },
    };

    // 💡 수정: visitedCount 대신 conditionKey(상태 키)를 받습니다.
    public static List<string> GetDialogueLines(string id, string conditionKey = "Default")
    {
        if (DialogueMapByState.ContainsKey(id))
        {
            var stateMap = DialogueMapByState[id];

            // 요청한 상태 키에 맞는 대사가 있는지 확인
            if (stateMap.ContainsKey(conditionKey))
            {
                return stateMap[conditionKey];
            }

            // 만약 요청한 키(예: "HasCircuit")가 없으면 "Default" 대사라도 내보냄 (폴백)
            if (stateMap.ContainsKey("Default"))
            {
                return stateMap["Default"];
            }
        }

        UnityEngine.Debug.LogWarning($"대사를 찾을 수 없습니다. Room: {id}, Key: {conditionKey}");
        return null;
    }
}