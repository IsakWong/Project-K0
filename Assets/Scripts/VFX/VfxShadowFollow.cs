using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class VfxShadowFollow : MonoBehaviour
    {
        public SkinnedMeshRenderer[] skinList;
        public float DeltaDistance = 0.25f;
        public float ShadowLifetime = 0.25f;
        private Vector3 oldPos;
        public Material mat;

        protected void Start()
        {
            skinList = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        protected void FixedUpdate()
        {
            if (Vector3.Distance(transform.position, oldPos) > DeltaDistance)
            {
                oldPos = transform.position;
                for (int i = 0; i < skinList.Length; i++)
                {
                    Mesh mesh = new Mesh();
                    skinList[i].BakeMesh(mesh);
                    GameObject go = new GameObject();
                    MeshFilter mf = go.AddComponent<MeshFilter>();
                    mf.mesh = mesh;
                    MeshRenderer mr = go.AddComponent<MeshRenderer>();
                    var materials = new Material[skinList[i].materials.Length];
                    for (int j = 0; j < skinList[i].materials.Length; j++)
                    {
                        materials[j] = new Material(mat);
                    }

                    mr.materials = materials;
                    GameObject.Destroy(go, ShadowLifetime);
                    go.transform.position = gameObject.transform.position;
                    go.transform.rotation = gameObject.transform.rotation;
                }
            }
        }
    }
}