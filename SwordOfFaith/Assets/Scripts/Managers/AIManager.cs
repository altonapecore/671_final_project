using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    // Singleton
    public static AIManager Instance;
    // Inspector Assigned Variables
    public GameObject playerOne;
    public GameObject playerTwo;
    [SerializeField] GameObject werewolfPrefab;
    [SerializeField] List<Transform> leftMapEnemySpawns;
    [SerializeField] List<Transform> rightMapEnemySpawns;
    [SerializeField] [Range(0.5f, 10.0f)] float spawnDelay = 3.0f;
    // Public Variables

    [Header("AI Spawn Settings")]
    [Space(10)]
    public EnemySpawnMode enemySpawnMode = EnemySpawnMode.Points;
    public Color minDistanceGizmoColor, maxDistanceGizmoColor;
    public int maxEnemiesPerSide = 20;
    public int minimumSpawnDistanceFromPlayer, maximumSpawnDistanceFromPlayer;

    public enum EnemySpawnMode { Points, Random }
    //[HideInInspector]
    public List<GameObject> leftSideEnemies;
    //[HideInInspector]
    public List<GameObject> rightSideEnemies;
    [HideInInspector]
    public bool leftSideActive = true, rightSideActive = true;
    // Private variables
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();
    void Awake()
    {
        if (!AIManager.Instance)
        {
            //Debug.Log("Setting Singleton");
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        leftSideEnemies = new List<GameObject>();
        rightSideEnemies = new List<GameObject>();
        // Spawn enemies on either side
        StartCoroutine(SpawnEnemies());
    }

    /*
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            SpawnRandomEnemyOnLeftSide();
        }
    }
    */

    /// <summary>
    /// Retrieves a registered AI state machine based on a key value
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;
        if (_stateMachines.TryGetValue(key, out machine))
        {
            return machine;
        }

        return null;
    }

    // --------------------------------------------------------------------
    // Name	:	RegisterAIStateMachine
    // Desc	:	Stores the passed state machine in the dictionary with
    //			the supplied key
    // --------------------------------------------------------------------
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if (!_stateMachines.ContainsKey(key))
        {
            _stateMachines[key] = stateMachine;
        }
    }

    public void UnregisterAIStateMachine(int key)
    {
        if (!_stateMachines.ContainsKey(key))
        {
            _stateMachines.Remove(key);
        }
    }

    public IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(spawnDelay);

        int randomInteger = Random.Range(0, 2);
        switch (randomInteger)
        {
            case 0:
                if (leftSideActive)
                {
                    SpawnSideEnemies(leftMapEnemySpawns);
                }
                break;
            case 1:
                if (rightSideActive)
                {
                    SpawnSideEnemies(rightMapEnemySpawns);
                }
                break;
        }

        if (leftSideActive || rightSideActive)
        {
            StartCoroutine(SpawnEnemies());
        }
    }

    private void SpawnSideEnemies(List<Transform> sideSpawn)
    {
        if (sideSpawn == leftMapEnemySpawns)
        {
            if (leftSideEnemies.Count > maxEnemiesPerSide-1) { return; }
        }
        else
        {
            if (rightSideEnemies.Count > maxEnemiesPerSide-1) { return; }
        }

        if (enemySpawnMode == EnemySpawnMode.Points)
        {
            int spawnPointIndex = Random.Range(0, sideSpawn.Count);
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(sideSpawn[spawnPointIndex].position, out navMeshHit, 5.0f, NavMesh.AllAreas))
            {
                DoSetEnemySpawn(sideSpawn, sideSpawn[spawnPointIndex].position, sideSpawn[spawnPointIndex].rotation);
            }
        }
        else
        {
            SpawnRandomEnemyOnSide(sideSpawn);
        }
    }

    public void FreezeSideEnemies(List<GameObject> sideSpawn, bool freeze)
    {
        for (int i = 0; i < sideSpawn.Count; i++)
        {
            if (freeze)
            {
                sideSpawn[i].GetComponent<AIWerewolfStateMachine>().speed = 0;
                sideSpawn[i].GetComponent<AIWerewolfState_Pursuit>()._speed = 0;
                sideSpawn[i].GetComponent<AIWerewolfState_Attack>()._speed = 0;
            }
            else
            {
                sideSpawn[i].GetComponent<AIWerewolfStateMachine>().speed = 1;
                sideSpawn[i].GetComponent<AIWerewolfState_Pursuit>()._speed = 2;
                sideSpawn[i].GetComponent<AIWerewolfState_Attack>()._speed = 1;
            }
        }

        if (sideSpawn == leftSideEnemies)
        {
            if (freeze == true)
            {
                leftSideActive = false;
            }
            else
            {
                leftSideActive = true;
            }
        }
        else
        {
            if (freeze == true)
            {
                rightSideActive = false;
            }
            else
            {
                rightSideActive = true;
            }
        }
    }

    public void KillSideEnemies(List<GameObject> sideSpawn)
    {
        do
        {
            for (int i = sideSpawn.Count - 1; i > -1; i--)
            {
                sideSpawn[i].GetComponent<AIWerewolfStateMachine>().Kill();
            }
        }
        while(sideSpawn.Count > 0);
    }

    public void DoSetEnemySpawn(List<Transform> sideSpawn, Vector3 spawnPos, Quaternion spawnRot)
    {
        GameObject enemy = Instantiate(werewolfPrefab, spawnPos, spawnRot);
        AIStateMachine stateMachine = enemy.GetComponent<AIStateMachine>();
        if (stateMachine)
        {
            if (sideSpawn == leftMapEnemySpawns)
            {
                stateMachine.isLeftMapEnemy = true;
                stateMachine.AssignPlayerTarget(playerOne.GetComponent<PlayerControl>());
                leftSideEnemies.Add(enemy.gameObject);
            }
            else
            {
                stateMachine.isLeftMapEnemy = false;
                stateMachine.AssignPlayerTarget(playerTwo.GetComponent<PlayerControl>());
                rightSideEnemies.Add(enemy.gameObject);
            }
        }
    }
    
    public void SpawnRandomEnemyOnSide(List<Transform> sideSpawn)
    {
        GameObject player;

        // Make sure we can still spawn enemies
        if (sideSpawn == leftMapEnemySpawns)
        {
            if (leftSideEnemies.Count > maxEnemiesPerSide-1) { return; }
            player = playerOne;
        }
        else
        {
            if (rightSideEnemies.Count > maxEnemiesPerSide-1) { return; }
            player = playerTwo;
        }

        Vector3 spawnPos = GetRandomCircleSpawnPosisiton(sideSpawn);

        GameObject enemy = Instantiate(werewolfPrefab, spawnPos, Quaternion.LookRotation(spawnPos - player.transform.position));
        AIStateMachine stateMachine = enemy.GetComponent<AIStateMachine>();
        if (stateMachine)
        {
            if (sideSpawn == leftMapEnemySpawns)
            {
                stateMachine.isLeftMapEnemy = true;
                stateMachine.AssignPlayerTarget(playerOne.GetComponent<PlayerControl>());
                leftSideEnemies.Add(enemy.gameObject);
            }
            else
            {
                stateMachine.isLeftMapEnemy = false;
                stateMachine.AssignPlayerTarget(playerTwo.GetComponent<PlayerControl>());
                rightSideEnemies.Add(enemy.gameObject);
            }
        }
    }

    public Vector3 GetRandomCircleSpawnPosisiton(List<Transform> sideSpawn)
    {
        Vector3 playerPos;
        Vector3 navMeshPos;
        NavMeshHit navMeshHit;

        do
        {

            if (sideSpawn == leftMapEnemySpawns)
            {
                playerPos = playerOne.transform.position;
            }
            else
            {
                playerPos = playerTwo.transform.position;
            }

            navMeshPos = RandomPointOnCircleEdge(Random.Range(minimumSpawnDistanceFromPlayer, maximumSpawnDistanceFromPlayer)) + playerPos;

        } while (NavMesh.SamplePosition(navMeshPos, out navMeshHit, 5.0f, NavMesh.AllAreas) != true);

        return navMeshPos;
    }

    private Vector3 RandomPointOnCircleEdge(float radius)
    {
        var vector2 = Random.insideUnitCircle.normalized * radius;
        return new Vector3(vector2.x, 0, vector2.y);
    }

    private void OnDrawGizmos()
    {
        if(enemySpawnMode == EnemySpawnMode.Random)
        {
            Gizmos.color = maxDistanceGizmoColor;
            //PlayerOne
            Gizmos.DrawLine(playerOne.transform.position, playerOne.transform.position + (playerOne.transform.forward * maximumSpawnDistanceFromPlayer));
            Gizmos.DrawLine(playerOne.transform.position, playerOne.transform.position + (playerOne.transform.right * maximumSpawnDistanceFromPlayer));

            Gizmos.DrawLine(playerOne.transform.position, playerOne.transform.position + (-1 * (playerOne.transform.forward * maximumSpawnDistanceFromPlayer)));
            Gizmos.DrawLine(playerOne.transform.position, playerOne.transform.position + (-1 * (playerOne.transform.right * maximumSpawnDistanceFromPlayer)));

            //PlayerTwo
            Gizmos.DrawLine(playerTwo.transform.position, playerTwo.transform.position + (playerTwo.transform.forward * maximumSpawnDistanceFromPlayer));
            Gizmos.DrawLine(playerTwo.transform.position, playerTwo.transform.position + (playerTwo.transform.right * maximumSpawnDistanceFromPlayer));

            Gizmos.DrawLine(playerTwo.transform.position, playerTwo.transform.position + (-1 * (playerTwo.transform.forward * maximumSpawnDistanceFromPlayer)));
            Gizmos.DrawLine(playerTwo.transform.position, playerTwo.transform.position + (-1 * (playerTwo.transform.right * maximumSpawnDistanceFromPlayer)));

            Gizmos.color = minDistanceGizmoColor;
            //PlayerOne
            Gizmos.DrawLine(playerOne.transform.position, playerOne.transform.position + (playerOne.transform.forward * minimumSpawnDistanceFromPlayer));
            Gizmos.DrawLine(playerOne.transform.position, playerOne.transform.position + (playerOne.transform.right * minimumSpawnDistanceFromPlayer));

            Gizmos.DrawLine(playerOne.transform.position, playerOne.transform.position + (-1 * (playerOne.transform.forward * minimumSpawnDistanceFromPlayer)));
            Gizmos.DrawLine(playerOne.transform.position, playerOne.transform.position + (-1 * (playerOne.transform.right * minimumSpawnDistanceFromPlayer)));

            //PlayerTwo
            Gizmos.DrawLine(playerTwo.transform.position, playerTwo.transform.position + (playerTwo.transform.forward * minimumSpawnDistanceFromPlayer));
            Gizmos.DrawLine(playerTwo.transform.position, playerTwo.transform.position + (playerTwo.transform.right * minimumSpawnDistanceFromPlayer));

            Gizmos.DrawLine(playerTwo.transform.position, playerTwo.transform.position + (-1 * (playerTwo.transform.forward * minimumSpawnDistanceFromPlayer)));
            Gizmos.DrawLine(playerTwo.transform.position, playerTwo.transform.position + (-1 * (playerTwo.transform.right * minimumSpawnDistanceFromPlayer)));
        }
    }
}
