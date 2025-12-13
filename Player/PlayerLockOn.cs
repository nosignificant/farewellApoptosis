using UnityEngine;

public class PlayerLockOn : MonoBehaviour
{
    public Camera mainCam;
    public Creature targetCreature;
    public float rotationSpeed = 5f;
    public Vector3 aimOffset = new Vector3(0, 1.0f, 0);

    private bool isTracking = false;

    void LateUpdate()
    {
        if (targetCreature == null) return;
        else
        {
            if (isTracking)
            {
                LookAtTarget();
            }
        }
    }

    public bool LockOnOff(bool onOff)
    {
        if (onOff)
        {
            targetCreature = FindCreatureInScreen();
            if (targetCreature != null)
            {
                isTracking = true;
                LookAtTarget();
                return true;
            }
            return false;
        }
        else
        {
            isTracking = false;
            targetCreature = null;
            return false;
        }
    }


    Creature FindCreatureInScreen()
    {
        Vector2 ScreenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Creature closestCreature = null;
        float minDistance = float.MaxValue;
        if (RoomManager.Instance != null)
        {
            string currentRoomID = Player.Instance.roomID;
            Room currentRoom = RoomManager.Instance.GetRoom(currentRoomID);
            if (currentRoom != null)
            {
                foreach (Creature creature in currentRoom.activeCreatures)
                {
                    if (creature.IsInScreenCenter(ScreenCenter))
                    {
                        float dist = Vector3.Distance(Player.Instance.transform.position, creature.transform.position);

                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closestCreature = creature;
                        }
                    }
                }
            }
        }
        return closestCreature;
    }

    void LookAtTarget()
    {
        // 💡 [핵심 수정 1] 기준점 변경
        // 내 몸통(transform)이 아니라 카메라(mainCam) 기준으로 계산해야 정확합니다.

        // A. 목표 지점
        Vector3 targetPos = targetCreature.transform.position + aimOffset;

        // B. 방향 벡터 (타겟 - 카메라 위치)
        Vector3 dir = targetPos - mainCam.transform.position;

        if (dir != Vector3.zero)
        {
            // C. 회전값 생성
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // 💡 [핵심 수정 2] 카메라를 직접 회전시킴!
            // transform.rotation -> mainCam.transform.rotation
            mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // (선택 사항) 몸통도 같이 돌리고 싶다면 Y축만 따서 transform.rotation에 적용해야 함
            // 하지만 일단 카메라만 돌려도 락온은 됩니다.
        }
    }

}