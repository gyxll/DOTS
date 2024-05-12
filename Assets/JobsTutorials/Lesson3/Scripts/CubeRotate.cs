using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.DOD.Lesson3
{
    public struct RotateSpeed:IComponentData
    {
        public float speed;
    }
    public class CubeRotate : MonoBehaviour
    {
        public float rotateSpeed;
        public class CubeRotateBaker : Baker<CubeRotate>
        {
            public override void Bake(CubeRotate authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var rotateData = new RotateSpeed()
                {
                    speed = math.radians(authoring.rotateSpeed)
                };
                AddComponent(entity, rotateData);
            }
        }
    }

}
