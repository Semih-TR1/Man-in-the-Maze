using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 2f;
    private bool hasAttacked = false;

    protected override void Start()
    {
        base.Start();

        if (agent != null)
        {
            // Düþmanýn karakterin içine gömülmeyip saldýrý mesafesinde durmasýný saðlar
            agent.stoppingDistance = attackRange - 0.3f;
        }
    }

    protected override void Update()
    {
        // Ana sýnýftaki (Enemy) Move() metodunu çalýþtýrarak oyuncuyu takip eder
        base.Update();

        if (player == null || hasAttacked) return;

        // Oyuncu ile düþman arasýndaki mesafeyi ölçer
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
    }

    protected override void Attack()
    {
        hasAttacked = true;
        agent.isStopped = true;

        Debug.Log("Yakýncý düþman vurdu! Oyun Bitti.");
        // Ýleride buraya Game Over ekranýný tetikleyen kod gelecek
    }
}