using Panda;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AllyAI : MonoBehaviour
{
    [SerializeField] private Transform enemy;
    [SerializeField] private Transform player;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Droid settings")]
    [Min(0.0f)]
    [SerializeField] private float wanderRadius = 15.0f;
    [SerializeField] private float visibleRange = 80.0f;
    [SerializeField] private float shotRange = 45.0f;
    
    private Vector3 destination; // The movement destination.
    private Vector3 targetPos;      // The position to aim to.
    
    [SerializeField] private float health = 100.0f;
    private float rotSpeed = 5.0f;
    private Camera _camera;
    private NavMeshAgent agent;
    

    void Start()
    {
        _camera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        InvokeRepeating(nameof(UpdateHealth), 5, 0.5f);
    }

    void Update()
    {
        Vector3 healthBarPos = _camera.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);
    }

    void UpdateHealth()
    {
        if (health < 100)
            health++;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("bullet"))
        {
            health -= 10;
        }
    }
    [Task]
    public void PickRandomDestinationNearPlayer()
    {
        float randomX = Random.Range(player.position.x - wanderRadius, player.position.x + wanderRadius);
        float randomZ = Random.Range(player.position.z - wanderRadius, player.position.z + wanderRadius);
        Vector3 dest = new Vector3(randomX, 0, randomZ);
        agent.SetDestination(dest);
        Task.current.Succeed();
    }
    [Task]
    public void PickDestination(float x, float z)
    {
        Vector3 dest = new Vector3(x, 0, z);
        agent.SetDestination(dest);
        Task.current.Succeed();
    }
    [Task]
    public void MoveToDestination()
    {
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }
    [Task]
    public void TargetEnemy()
    {
        targetPos = enemy.transform.position;
        Task.current.Succeed();
    }
    [Task]
    bool Turn(float angle)
    {
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;
        targetPos = p;
        return true;
    }
    [Task]
    public void LookAtTarget()
    {
        Vector3 direction = targetPos - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);
        
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("angle={0}", Vector3.Angle(this.transform.forward, direction));

        if (Vector3.Angle(this.transform.forward, direction) < 5.0f)
        {
            Task.current.Succeed();
        }
    }
    [Task]
    public bool Fire()
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);
        return true;
    }
    [Task]
    bool SeeEnemy()
    {
        Vector3 distance = enemy.transform.position - this.transform.position;
        RaycastHit hit;
        bool seeWall = false;

        Task.current.debugInfo = $"GO NAME: ";
        Debug.DrawRay(this.transform.position, distance, Color.yellow);
        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            Debug.Log($"GO NAME: {hit.collider.gameObject.name}");
            if (hit.collider.gameObject.CompareTag("wall"))
            {
                Task.current.debugInfo = $"GO NAME: {hit.collider.gameObject.name}";
                seeWall = true;
            }
            else
            {
                Task.current.debugInfo = $"GO NAME: {hit.collider.gameObject.name}";
            }
        }
        
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("wall={0}", seeWall);

        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }
    [Task]
    public bool IsHealthLessThan(float health)
    {
        return this.health < health;
    }
    [Task]
    public bool IsPlayerHealthLessThan(float health)
    {
        return player.GetComponent<PlayerHealth>().GetCurrentHealth() < health;
    }

    [Task]
    public void CoverPlayer()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 enemyPos = enemy.transform.position;
        
        float newPosX = playerPos.x + ( enemyPos.x - playerPos.x ) / 2;
        float newPosZ = playerPos.z + ( enemyPos.z - playerPos.z ) / 2;
        agent.stoppingDistance = 0;
        Vector3 dest = new Vector3(newPosX, 0, newPosZ);
        Debug.DrawRay(this.transform.position, dest, Color.yellow);
        agent.SetDestination(dest);
        Task.current.Succeed();
    }
    [Task]
    public bool Explode()
    {
        Destroy(healthBar.gameObject);
        Destroy(this.gameObject);
        return true;
    }
    [Task]
    public void SetTargetDestination()
    {
        agent.SetDestination(targetPos);
        Task.current.Succeed();
    }
    [Task]
    public bool ShotLinedUp()
    {
        Vector3 distance = targetPos - this.transform.position;
        if (distance.magnitude < shotRange && Vector3.Angle(this.transform.forward, distance) < 1.0f)
            return true;
        else
            return false;
    }
}
