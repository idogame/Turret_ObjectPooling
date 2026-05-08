using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TurretDemo
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform enemyRoot;

        [Header("Spawn Settings")]
        [SerializeField][Min(1)] private int initialSpawnCount = 4;
        [SerializeField][Min(1)] private int maxAliveCount = 8;
        [SerializeField][Min(0.1f)] private float spawnIntervalSeconds = 1f;
        [SerializeField] private Transform lookAtCenter;

        [Header("Enemy Stats")]
        [SerializeField][Min(0.1f)] private float enemyMoveSpeedUnitsPerSecond = 5f;
        [SerializeField][Min(0.1f)] private float enemyLifeTimeSeconds = 10f;

        private ObjectPool<GameObject> enemyPool;
        private Coroutine spawnCoroutine;
        private readonly WaitForSeconds spawnWait = new WaitForSeconds(1f); // 초기값, 이후 업데이트

        private void Awake()
        {
            enemyPool = new ObjectPool<GameObject>(
                createFunc: CreateEnemy,
                actionOnGet: OnGetEnemy,
                actionOnRelease: OnReleaseEnemy,
                actionOnDestroy: OnDestroyEnemy,
                collectionCheck: true,
                defaultCapacity: initialSpawnCount,
                maxSize: 20
            );
        }

        private void Start()
        {
            // 초기 스폰
            for (int i = 0; i < initialSpawnCount; i++)
            {
                TrySpawnOneEnemy();
            }

            // 스폰 루틴 시작
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            // 캐싱 
            var interval = new WaitForSeconds(spawnIntervalSeconds);

            while (true)
            {
                // 최대 개수 제한 체크
                if (enemyPool.CountActive < maxAliveCount)
                {
                    TrySpawnOneEnemy();
                }

                yield return interval;
            }
        }

        #region Pool Handlers
        private GameObject CreateEnemy() => Instantiate(enemyPrefab, enemyRoot);
        private void OnGetEnemy(GameObject enemy) => enemy.SetActive(true);
        private void OnReleaseEnemy(GameObject enemy) => enemy.SetActive(false);
        private void OnDestroyEnemy(GameObject enemy) => Destroy(enemy);
        #endregion

        private bool TrySpawnOneEnemy()
        {
            if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return false;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (spawnPoint == null) return false;

            Quaternion spawnRotation = spawnPoint.rotation;
            if (lookAtCenter != null)
            {
                Vector3 toCenter = lookAtCenter.position - spawnPoint.position;
                if (toCenter.sqrMagnitude > 1e-8f)
                {
                    spawnRotation = Quaternion.LookRotation(toCenter.normalized, Vector3.up);
                }
            }

            GameObject spawned = enemyPool.Get();
            spawned.transform.position = spawnPoint.position;
            spawned.transform.rotation = spawnRotation;

            if (spawned.TryGetComponent<EnemyLinearMover>(out var mover))
            {
                mover.Initialize(enemyMoveSpeedUnitsPerSecond, enemyLifeTimeSeconds, enemyPool);
            }

            return true;
        }
    }
}