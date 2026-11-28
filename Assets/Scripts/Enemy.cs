using UnityEngine;
using UnityEngine.AI; // NavMeshAgent kullanmak için bu kütüphane þart

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int health = 1; // Tek sýkýmlýk can
    [SerializeField] protected float moveSpeed = 4f;

    protected NavMeshAgent agent;
    protected Transform player;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        // Sahnedeki "Player" etiketli objeyi otomatik bulur
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
    }

    protected virtual void Update()
    {
        if (player == null) return;

        Move();
    }

    // Bize doðru gelme kýsmý
    protected virtual void Move()
    {
        agent.SetDestination(player.position);
    }

    // Düþmanýn hasar alma fonksiyonu
    public virtual void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    // Her düþmanýn saldýrý þekli farklý olacaðý için gövdesini alt sýnýflar dolduracak
    protected abstract void Attack();
}