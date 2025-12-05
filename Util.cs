using UnityEngine;
using System.Collections.Generic;


public static class Util
{
    // ---------------------- transform - move ------------------------
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

    // ---------------------- rigidbody - move ------------------------

    public static void towards(Rigidbody rb, float speed, Vector3 dir)
    {
        // 물리 연산이므로 Time.fixedDeltaTime 사용 권장 (FixedUpdate에서 호출 시)
        // 하지만 Update에서 호출해도 작동은 합니다.
        float deltaTime = Time.deltaTime;

        Vector3 newPosition = rb.position + dir * speed * deltaTime;
        rb.MovePosition(newPosition);
    }

    public static void moveBack(Rigidbody rb, float speed, Vector3 dir)
    {
        float deltaTime = Time.deltaTime;

        Vector3 newPosition = rb.position - dir * speed * deltaTime;
        rb.MovePosition(newPosition);
    }

    // ---------------------- room ------------------------
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

    public static List<Spawner> FindCurrentRoomSpawner(string roomID)
    {
        return RoomManager.Instance.GetSpawnersInRoom(roomID);
    }
}