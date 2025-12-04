using UnityEngine;
using System.Collections;
public class Food : MonoBehaviour
{
    public int foodHealth;
    public int MAX_HP = 3;
    public float respawnTime = 10f;

    private Renderer meshRenderer;
    private Collider foodCollider;

    void Start()
    {
        foodHealth = MAX_HP;
        meshRenderer = GetComponent<Renderer>();
        foodCollider = GetComponent<Collider>();
    }


    public void TakeBite(int damage)
    {
        foodHealth -= damage;

        if (foodHealth <= 0)
        {
            StartCoroutine(HideAndRespawn());
        }
    }


    IEnumerator HideAndRespawn()
    {
        meshRenderer.enabled = false;
        foodCollider.enabled = false;

        yield return new WaitForSeconds(respawnTime);

        foodHealth = MAX_HP;
        meshRenderer.enabled = true;
        foodCollider.enabled = true;
    }
}
