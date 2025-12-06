using UnityEngine;
using System.Collections;
public class Food : Creature
{
    public float respawnTime = 10f;
    private Renderer meshRenderer;
    private Collider foodCollider;

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        foodCollider = GetComponent<Collider>();
        currentRoom = Util.FindCurrentRoom(transform.position);
        if (currentRoom != null) currentRoom.OnCreatureEnter(this);
    }

    protected override void Die()
    {
        StartCoroutine(HideAndRespawn());
    }
    IEnumerator HideAndRespawn()
    {
        meshRenderer.enabled = false;
        foodCollider.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        currentHP = MAX_HP;
        meshRenderer.enabled = true;
        foodCollider.enabled = true;
    }
}
