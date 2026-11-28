using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StalkerEnemy : Enemy
{
    [Header("Camera Reference")]
    [SerializeField] private Camera playerCamera; // Boþ býrakýrsan MainCamera'yý otomatik bulur

    [Header("Stalker Speeds")]
    [SerializeField] private float stalkSpeed = 6f;      // Gizlice yaklaþma hýzý
    [SerializeField] private float ambushSpeed = 11.5f;  // Kamera alaný dýþýndayken saldýrý hýzý
    [SerializeField] private float retreatSpeed = 5f;    // Kameraya yakalanýnca geri çekilme hýzý

    [Header("Stalker Distances")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float lookAheadDistance = 1.2f; // Kamera alanýna girmeden kaç metre önce dursun
    [SerializeField] private float breadcrumbStep = 1.0f;    // Rota izi býrakma aralýðý

    private Collider enemyCollider;
    private List<Vector3> pathHistory = new List<Vector3>();
    private bool isRetreating = false;
    private bool hasAttacked = false;

    protected override void Awake()
    {
        base.Awake();
        enemyCollider = GetComponent<Collider>();
    }

    protected override void Start()
    {
        base.Start();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (agent != null)
        {
            agent.speed = stalkSpeed;
        }

        pathHistory.Add(transform.position);
    }

    protected override void Update()
    {
        if (player == null || playerCamera == null || hasAttacked) return;

        // Stalker'ýn kendi gövdesi þu an kameranýn çizdiði alanda mý?
        Vector3 bodySize = enemyCollider != null ? enemyCollider.bounds.size : new Vector3(1f, 2f, 1f);
        Vector3 bodyCenter = enemyCollider != null ? enemyCollider.bounds.center : transform.position + Vector3.up;
        bool amIVisibleOnCamera = IsBoxInCameraView(bodyCenter, bodySize);

        if (amIVisibleOnCamera)
        {
            isRetreating = true;
        }
        else
        {
            // Kamera alanýndan çýktý (duvar arkasýna girdi veya oyuncu kafasýný çevirdi)
            isRetreating = false;
        }

        if (isRetreating)
        {
            HandleRetreat();
        }
        else
        {
            HandleStalkAndAmbush(bodySize);
        }
    }

    private void HandleStalkAndAmbush(Vector3 bodySize)
    {
        // Hedefi güncelle ki NavMesh bir sonraki adým yönünü (steeringTarget) hesaplasýn
        agent.SetDestination(player.position);

        // Stalker'ýn bir sonraki adýmda basacaðý noktayý hesapla
        Vector3 moveDir = (agent.steeringTarget - transform.position);
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            moveDir.Normalize();
            Vector3 nextStepCenter = transform.position + moveDir * lookAheadDistance + Vector3.up * (bodySize.y * 0.5f);

            // Eðer bir sonraki adýmý kameranýn görüþ alanýna girecekse ORADA DUR, kadraja girme
            if (IsBoxInCameraView(nextStepCenter, bodySize))
            {
                agent.isStopped = true;
                return;
            }
        }

        // Önü kamera alanýna girmiyor -> Yaklaþmaya devam et
        agent.isStopped = false;
        agent.updateRotation = true;

        // Arada duvar varsa normal hýzda süzül, ayný koridordaysak (oyuncunun sýrtý dönük) hýzlan
        bool hasWallBetween = Physics.Linecast(playerCamera.transform.position, transform.position + Vector3.up);
        agent.speed = hasWallBetween ? stalkSpeed : ambushSpeed;

        RecordBreadcrumb();

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            Attack();
        }
    }

    private void HandleRetreat()
    {
        agent.isStopped = false;
        agent.updateRotation = false;
        agent.speed = retreatSpeed;
        FacePlayer();

        if (pathHistory.Count > 0)
        {
            Vector3 lastPoint = pathHistory[pathHistory.Count - 1];
            agent.SetDestination(lastPoint);

            if (Vector3.Distance(transform.position, lastPoint) < 0.7f)
            {
                pathHistory.RemoveAt(pathHistory.Count - 1);
            }
        }
        else
        {
            Vector3 awayDir = (transform.position - player.position).normalized;
            awayDir.y = 0f;
            if (NavMesh.SamplePosition(transform.position + awayDir * 3f, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    // Verilen kutunun hem kameranýn görüþ piramidinde olup olmadýðýný hem de duvar arkasýnda kalýp kalmadýðýný kontrol eder
    private bool IsBoxInCameraView(Vector3 center, Vector3 size)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        Bounds bounds = new Bounds(center, size);

        // 1. Kameranýn çizdiði piramidin dýþýndaysa direkt false
        if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
        {
            return false;
        }

        // 2. Piramidin içindeyse arada duvar var mý diye bak
        Vector3 camPos = playerCamera.transform.position;
        if (Physics.Linecast(camPos, center, out RaycastHit hit))
        {
            // Iþýn Stalker'ýn kendi collider'ýna çarptýysa arada duvar yok demektir
            if (hit.transform == transform) return true;

            // Baþka bir objeye (duvara) çarptýysa kamera göremiyor demektir
            return false;
        }

        // Hiçbir engele çarpmadý ve kamera piramidinin içinde -> Görünüyor!
        return true;
    }

    private void RecordBreadcrumb()
    {
        if (pathHistory.Count == 0 || Vector3.Distance(transform.position, pathHistory[pathHistory.Count - 1]) >= breadcrumbStep)
        {
            pathHistory.Add(transform.position);

            if (pathHistory.Count > 60)
            {
                pathHistory.RemoveAt(0);
            }
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    protected override void Attack()
    {
        hasAttacked = true;
        agent.isStopped = true;
        Debug.Log("Stalker arkandan yaklaþtý ve vurdu! Oyun Bitti.");
    }
}