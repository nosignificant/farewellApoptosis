using UnityEngine;


public static class Util
{
    public static Vector3 GetDirectionTo(Transform current, Transform target)
    {
        return (target.position - current.position).normalized;
    }

    public static void towards(Transform targetTransform, float speed, Vector3 dir, float accel = 1f)
    {
        targetTransform.position += dir * speed * accel * Time.deltaTime;
    }

    public static void moveBack(Transform targetTransform, float speed, Vector3 dir, float accel = 1f)
    {
        targetTransform.position -= dir * speed * accel * Time.deltaTime;
    }

    public static Room FindCurrentRoom(Vector3 position, float searchRadius = 10f)
    {
        Collider[] hits = Physics.OverlapSphere(position, searchRadius);

        if (hits == null || hits.Length == 0) return null;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Room"))
            {
                Room room = hit.GetComponent<Room>();
                if (room != null) return room;
            }
        }

        return null;
    }

    public static Spawner FindCurrentRoomSpawner(string roomID)
    {
        return RoomManager.Instance.GetSpawnersInRoom(roomID);
    }
}