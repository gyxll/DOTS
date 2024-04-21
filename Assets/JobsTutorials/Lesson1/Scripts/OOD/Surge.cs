using UnityEngine;
using UnityEngine.Pool;

namespace Jobs.OOD
{
    public class Surge : MonoBehaviour
    {
        /// <summary>
        /// 立方体的原型
        /// </summary>
        public GameObject cubeArchtype;
        /// <summary>
        /// 出生区域
        /// </summary>
        public BoxCollider startArea;
        private Vector3 startPos;
        private Vector3 startRange;
        /// <summary>
        /// 终点区域
        /// </summary>
        public BoxCollider endArea;
        private Vector3 endPos;
        private Vector3 endRange;
        /// <summary>
        /// 总共生成数量，要根据生成的间隔tickTime、每次生成的数量generationNumPerTicktime、和移动速度moveSpeed动态设置，否则会出现生成停滞的问题
        /// </summary>
        [Range(10, 10000)] public int generationTotalNum = 500;
        /// <summary>
        /// 每次间隔时间生成的数量
        /// </summary>
        [Range(1, 60)] public int generationNumPerTicktime = 5;
        /// <summary>
        /// 生成的时间间隔
        /// </summary>
        [Range(0.1f, 1.0f)] public float tickTime = 0.2f;
        /// <summary>
        /// 旋转速度范围
        /// </summary>
        [Range(90, 180)] public float rotateRange = 180;
        /// <summary>
        /// 移动速度
        /// </summary>
        public float moveSpeed = 5;
        /// <summary>
        /// 立方体的对象池
        /// </summary>
        public static ObjectPool<GameObject> cubesPool;
        /// <summary>
        /// 下次更新时间
        /// </summary>
        private float nextUpdateTime = 0;

        // Start is called before the first frame update
        void Start()
        {
            cubesPool = new ObjectPool<GameObject>(CreteCube, OnGetCube, OnRecycleCube, OnDestroyCube, true, 10, generationTotalNum);
            startPos = startArea.transform.position;
            startRange = startArea.size * 0.5f;
            endPos = endArea.transform.position;
            endRange = endArea.size * 0.5f;
        }

        private void OnDestroyCube(GameObject obj)
        {
            Destroy(obj);
        }

        private void OnRecycleCube(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void OnGetCube(GameObject obj)
        {
            obj.SetActive(true);
            RandomMoveInfo(obj);
        }
        /// <summary>
        /// 创建单个立方体实例
        /// </summary>
        /// <returns></returns>
        private GameObject CreteCube()
        {
            var newCube = Instantiate(cubeArchtype);
            newCube.hideFlags = HideFlags.HideInHierarchy;
            newCube.AddComponent<RandomMoveAndRotate>();
            return newCube;
        }

        private Vector3 tempVec3 = Vector3.zero;
        /// <summary>
        /// 设置随机目标
        /// </summary>
        /// <param name="newCube"></param>
        private void RandomMoveInfo(GameObject newCube)
        {
            tempVec3.Set(Random.Range(-startRange.x, startRange.x), 0, Random.Range(-startRange.z, startRange.z));
            newCube.transform.position = startPos + tempVec3;
            var randomMoveAndRotate = newCube.GetComponent<RandomMoveAndRotate>();
            tempVec3.Set(Random.Range(-endRange.x, endRange.x), 0, Random.Range(-endRange.z, endRange.z));
            randomMoveAndRotate.targetPos = endPos + tempVec3;
            randomMoveAndRotate.rotateSpeed = Random.Range(-rotateRange, rotateRange);
            randomMoveAndRotate.moveSpeed = moveSpeed;
        }

        private void Update()
        {
            if (Time.time > nextUpdateTime)
            {
                for (int i = 0; i < generationNumPerTicktime; i++)
                {
                    if (cubesPool.CountAll < generationTotalNum)
                    {
                        cubesPool.Get();
                    }
                    else
                    {
                        nextUpdateTime += tickTime;
                        return;
                    }
                }
                nextUpdateTime += tickTime;
            }
        }

        private void OnDestroy()
        {
            cubesPool.Dispose();
        }
    }
}

