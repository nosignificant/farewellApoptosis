using System.Collections.Generic;

public static class DialogueData
{

    public static readonly Dictionary<string, Dictionary<string, List<string>>> DialogueMapByState =
        new Dictionary<string, Dictionary<string, List<string>>>()
    {

        {"tut_00", new Dictionary<string, List<string>>
            {
                // 기본 상태 (아무 조건 없을 때)
                {"startEvent", new List<string>
                    {
                        "안녕하세요. 저는 p53입니다.",
                        "시설 수리를 위한 교육을 시작합니다.",
                        "오른쪽에 보이는 것은 생물 부화기입니다.",
                        "회로를 수정함으로 조작함으로 여러 생물을 부화시킬 수 있습니다.",
                        "생물 부화기를 조작하십시오."
                    }
                },
                {"hasCircuit", new List<string>
                {
                    "잘하셨습니다.",
                    "생물 부화기는 회로를 연결, 조작해 다양한 생물을 탄생시킵니다.",
                    "다음 방으로 이동하십시오."
                }
            },
                {"endEvent", new List<string>
                    {

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
                {"startEvent", new List<string>
                    {
"이곳에 보이는 것은 생산 나무입니다.",
"그리고 오른 편에는 작동하지 않는 생물 부화기가 있습니다.",
"회로를 조정하여 해당 생물 부화기가 작동하게 하십시오."
                    }
                },
                // 2. 플레이어가 회로를 가지고 있을 때
                {"hasCircuit", new List<string>
                    {   "이곳에 보이는 것은 생산 나무입니다.",
                        "생물들은 이 나무에서 자라는 정보를 먹고 활동합니다.",
                        "이전 방에서 확득한 회로를 이쪽에 연결하십시오.",
                    }
                },
                // 3. 이미 수리가 끝났을 때
                {"repaired", new List<string>
                    {
                        "잘 하셨습니다.",
                        "생산 나무가 주변에 있으면, 생물은 반응하고, 행동합니다.",
                        "다음 구역으로 이동하십시오."
                    }
                }
            }
        },
        {"tut_02", new Dictionary<string, List<string>>
        {
{"startEvent", new List<string>
{
"이곳에는 또 다른 방식으로 활동하는 생물이 있습니다.",
"이 생물이 반응하는 방식을 알아내십시오."
}
            }
        }
            }
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