using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.DOD.Lesson4
{
    [BurstCompile]
    
    partial struct WaveCubeEntityMoveJob : IJobEntity
    {
        public double elapsedTime;
        void Execute(ref LocalTransform localTransform,in WaveCubeDistance cubeDistance)
        {
            float y = (float)math.sin(elapsedTime * 3f + cubeDistance.distance * 0.2f) * 9;
            localTransform.Position = new float3(localTransform.Position.x, y, localTransform.Position.z);
        }
    }
}
        
