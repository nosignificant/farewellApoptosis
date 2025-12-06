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
                    "회로를 분리한 후 다음 방으로 이동하십시오."
                }
            },
            }
        },

        // ----------------------------------------------------
        // Room: tut_01 (생산기 방)
        // ----------------------------------------------------
        {"tut_01", new Dictionary<string, List<string>>
            {

                // 2. 플레이어가 회로를 가지고 있을 때
                {"startEvent", new List<string>
                    {   "당신이 수리해야하는 곳은 생산 시설입니다.",
                        "생산 시설은 생산 나무와 생물들을 중심으로 작동합니다.",
                        "이전 방에서 확득할 수 있는 회로를 이쪽에 연결해보십시오.",
                    }
                },
                // 3. 이미 수리가 끝났을 때
                {"repaired", new List<string>
                    {
                        "잘 하셨습니다.",
                        "이 생물들은 생산 나무의 정보를 먹고, 감각 정보로 변환합니다.",
                        "저는 판단 시설 내에 1번 정보와 3번 정보가 충분히 많을 때, 원활하게 작동합니다.",
                        "그러나 현재 오류로 인해 시설 내의 감각 정보 항상성이 파괴되어 있습니다.",
                        "이를 원상복구 해야합니다.",
                    }
                }
            }
        },
{"tut_02", new Dictionary<string, List<string>>
            {
                {"startEvent", new List<string>
                    {
                        "이곳에는 또 다른 방식으로 활동하는 생물이 있습니다.",
                        "이 생물이 반응하는 방식을 알아내보십시오.",
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