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
                        "당신은 시설과 생물이 올바르게 작동하도록 수리해야합니다.",
                        "그러기 위해서는 우선 생물을 관찰해야 합니다.",
                        "생물을 바라보고 tab키를 눌러 관찰하십시오.",

                    }
                },
            {
                "lockedOn", new List<string>
                {
                    "",
                    "현재 상태는 상단, 이 생물이 가질 수 있는 다른 상태는 우측에 표기됩니다.",
                    "Tab키를 한 번 더 눌러 관찰을 중지하십시오.",
                    "",
                    "다음은 생물 부화기입니다.",
                    "생물 부화기는 시설에 필요한 기초 생물들을 부화시킵니다.",
                    "E키로 부화기를 열고 A, D 키로 생물 부화기를 조작하십시오."
                }
            },
                {"hasCircuit", new List<string>
                {
                    "",
                    "생물 부화기는 회로를 연결, 조작해 다양한 생물을 탄생시킵니다.",
                    "회로를 분리하고 ESC키를 눌러 조작을 중지하십시오.",
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
                        "이전 방에서 확득할 수 있는 회로를 이곳의 부화기에 연결하십시오.",
                    }
                },
                // 3. 이미 수리가 끝났을 때
                {"repaired", new List<string>
                    {
                        "이 생물들은 생산 나무의 정보를 먹고, 감각 정보로 변환합니다.",
                        "감각 정보는 중요합니다. 감각 정보는 시설이 원활하게 작동시키는 역할입니다.",
                        "특정 시설에서 요구하는 수치만큼 생산량을 복구해 시설을 수리하십시오."
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
        },
        {"tut_04", new Dictionary<string, List<string>>
            {
                {"endTutorial", new List<string>
                    {
                        "이 생물이 부화기에서 태어난 생물을 먹으면, 정보의 개수가 집계됩니다.",
                        "다음 방으로 이동하십시오."

                    }
                }
            }
        },
        {"tut_06", new Dictionary<string, List<string>>
            {
                {"startEvent", new List<string>
                    {
                        "이것으로 안내는 끝입니다...",
                        "생물이 반응하는 방식을 알아내고 시설이 기능하도록 하십시오.",
                        "저 생물에게는",
                        "",
                        "가까이 가지 마십시오",
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