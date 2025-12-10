using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string tutorialSceneName = "tutorial";

    // 시작 버튼에 연결할 함수
    public void OnClickStartGame()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }

    // (선택) 종료 버튼용
    public void OnClickExitGame()
    {
        Application.Quit();
    }
}