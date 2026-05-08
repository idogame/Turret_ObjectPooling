using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace TurretDemo
{
    [DisallowMultipleComponent]
    public sealed class EnemyLinearMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeedUnitsPerSecond = 5f;
        [SerializeField] private float lifeTimeSeconds = 10f;

        private readonly WaitForSeconds waitOneSecond = new WaitForSeconds(1.0f);

        private float remainingLifeSeconds;
        private ObjectPool<GameObject> managedPool;
        private Coroutine moveCoroutine;

        public void Initialize(float speedUnitsPerSecond, float lifeTime, ObjectPool<GameObject> pool)
        {
            moveSpeedUnitsPerSecond = speedUnitsPerSecond;
            lifeTimeSeconds = lifeTime;
            managedPool = pool;
        }

        private void OnEnable()
        {
            remainingLifeSeconds = lifeTimeSeconds;
            // 재활용 시 기존 코루틴 중복 방지 및 새로 시작
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveRoutine());
        }

        private IEnumerator MoveRoutine()
        {
            while (remainingLifeSeconds > 0)
            {
                // 이동 처리
                transform.Translate(Vector3.forward * moveSpeedUnitsPerSecond, Space.Self);

                // 수명 차감
                remainingLifeSeconds -= 1.0f;

                //캐싱해서 사용 ㅋ
                yield return waitOneSecond;
            }

            ReleaseToPool();
        }

        public void ReleaseToPool()
        {
            if (managedPool != null)
            {
                managedPool.Release(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
}