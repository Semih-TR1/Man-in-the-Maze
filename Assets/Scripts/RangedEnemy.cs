using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged Settings")]
    [SerializeField] private float aimDuration = 2f; // Ateþ etmeden önceki bekleme süresi
    private float currentAimTimer = 0f;
    private bool hasAttacked = false;

    protected override void Update()
    {
        // Burada base.Update() ÇAÐIRMIYORUZ.
        // Çünkü Uzakçý düþman oyuncuyu gördüðü an yürümeyi býrakýp niþan almalý.
        if (player == null || hasAttacked) return;

        if (CanSeePlayer())
        {
            // 1. Oyuncuyu görüyor: Olduðu yerde dur, oyuncuya dön ve 2 saniye say
            agent.isStopped = true;
            FacePlayer();

            currentAimTimer += Time.deltaTime;

            if (currentAimTimer >= aimDuration)
            {
                Attack();
            }
        }
        else
        {
            // 2. Arada duvar var: Sayacý sýfýrla ve ana sýnýftaki Move() ile kovalamaya devam et
            currentAimTimer = 0f;
            agent.isStopped = false;
            Move();
        }
    }

    private bool CanSeePlayer()
    {
        // Iþýný yerden biraz yukarýdan (göz hizasýndan) çekiyoruz ki zemine takýlmasýn
        Vector3 startPoint = transform.position + Vector3.up * 0.5f;
        Vector3 targetPoint = player.position + Vector3.up * 0.5f;

        // Ýki nokta arasýna görünmez bir çizgi çeker; ilk çarptýðý objeyi 'hit' içine kaydeder
        if (Physics.Linecast(startPoint, targetPoint, out RaycastHit hit))
        {
            // Çarptýðý ilk obje Player ise arada duvar yok demektir
            if (hit.transform == player || hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f; // Düþmanýn yukarý veya aþaðý devrilmesini engeller

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    protected override void Attack()
    {
        hasAttacked = true;
        Debug.Log("Uzakçý düþman 2 saniye niþan aldý ve vurdu! Oyun Bitti.");
        // Ýleride buraya ateþ efekti/sesi ve Game Over tetikleyicisi gelecek
    }
}