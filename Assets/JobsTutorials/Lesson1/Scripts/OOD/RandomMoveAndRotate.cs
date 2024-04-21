using Unity.Profiling;
using UnityEngine;

namespace Jobs.OOD
{
    public class RandomMoveAndRotate : MonoBehaviour
    {
        /// <summary>
        /// 距离判断阈值
        /// </summary>
        public const float DistanceFlag = 0.05f;
        /// <summary>
        /// 当前目标
        /// </summary>
        public Vector3 targetPos;
        /// <summary>
        /// 旋转速度
        /// </summary>
        public float rotateSpeed;
        /// <summary>
        /// 移动速度
        /// </summary>
        public float moveSpeed;

        static readonly ProfilerMarker profilerMarker = new("CubesMarch");

        // Update is called once per frame
        void Update()
        {
            using (profilerMarker.Auto())
            {
                var dir = targetPos - transform.position;
                if (dir.sqrMagnitude < DistanceFlag)
                {
                    Surge.cubesPool.Release(gameObject);
                }
                else
                {
                    transform.position += moveSpeed * Time.deltaTime * dir.normalized;
                    transform.eulerAngles += rotateSpeed * Time.deltaTime * Vector3.up;
                }
            }                
        }
    }

}
