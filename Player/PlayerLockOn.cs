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
        if (targetCreature != null && isTracking)
        {
            LookAtTarget();
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
                    else Debug.Log("뭐지 ");
                }
            }
        }
        return closestCreature;
    }

    void LookAtTarget()
    {
        Vector3 targetPos = targetCreature.transform.position + aimOffset;
        Vector3 dir = targetPos - mainCam.transform.position;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

}