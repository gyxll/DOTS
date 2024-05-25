using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.DOD.Lesson4
{
    [BurstCompile]
    partial struct WaveCubeEntityMoveJob : IJobEntity
    {
        public float elapsedTime;
        void Execute(ref LocalTransform localTransform,in WaveCubeDistance cubeDistance)
        {
            float y = math.sin(elapsedTime * 3f + cubeDistance.distance * 0.2f) * 9;
            localTransform.Position = new float3(localTransform.Position.x, y, localTransform.Position.z);
        }
    }
}
        
