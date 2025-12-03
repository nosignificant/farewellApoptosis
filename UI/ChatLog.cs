using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//채팅 로그가 어떻게 올라갈지에 관한 파일
public class ChatLog : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject messagePrefab;   // 패널 + TMP_Text 포함된 프리팹
    public int maxLines = 5;
    public float scrollDuration = 0.2f;
    public float messageLifeTime = 5f;

    public void Post(string text)
    {
        StartCoroutine(PostRoutine(text));
    }

    private IEnumerator PostRoutine(string text)
    {
        // 1) 새 줄 생성
        GameObject go = Instantiate(messagePrefab, content);
        TMP_Text msg = go.GetComponentInChildren<TMP_Text>(); // ← 자식에서 텍스트 찾기
        msg.text = text;

        // 2) 레이아웃 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        // 3) 스크롤 부드럽게 아래로
        float t = 0f;
        float start = scrollRect.verticalNormalizedPosition;
        float end = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / scrollDuration;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, end, t);
            yield return null;
        }

        while (content.childCount > maxLines)
        {
            Destroy(content.GetChild(0).gameObject);
        }

        StartCoroutine(RemoveAfter(go, messageLifeTime));
    }

    private IEnumerator RemoveAfter(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null)
            Destroy(target);
    }
}