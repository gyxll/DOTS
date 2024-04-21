using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Jobs.DOD
{
    [BurstCompile]
    struct RandomMoveAndRotateJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<Vector3> targetPosArr;
        [ReadOnly]
        public NativeArray<float> rotateSpeedArr;
        [ReadOnly]
        public NativeArray<float> moveSpeedArr;
        /// <summary>
        /// 是否抵达目的地
        /// </summary>
        [NativeDisableParallelForRestriction]
        public NativeArray<bool> isReachedArr;
        [ReadOnly]
        public float distanceFlag;
        [ReadOnly]
        public float deltaTime;


        public void Execute(int index, TransformAccess transform)
        {
            if (isReachedArr[index])
            {
                return;
            }
            var dir = targetPosArr[index] - transform.position;
            if (math.lengthsq(dir) < distanceFlag)
            {
                isReachedArr[index] = true;
            }
            else
            {
                float3 addValue = moveSpeedArr[index] * deltaTime * math.normalize(dir);
                var curPos = transform.position + new Vector3(addValue.x, addValue.y, addValue.z);
                var curRotate = transform.rotation.eulerAngles;

                curRotate += rotateSpeedArr[index] * deltaTime * Vector3.up;
                transform.SetPositionAndRotation(curPos, Quaternion.Euler(curRotate));
            }
        }

    }
}
