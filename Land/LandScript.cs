#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LandScript : MonoBehaviour
{
    public GameObject Prefab;
    public Transform Parent;

    [ContextMenu("PUT_PLANES")]
    public void PutPlanes()
    {
        int count = 1;
        for (int z = -1350; z <= 1350; z += 300)
        {
            for (int x = -1350; x <= 1350; x += 300)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(Prefab) as GameObject;

                obj.name = "Plane (" + count + ")";
                count++;

                Transform tran = obj.GetComponent<Transform>();
                tran.position = new Vector3(x, 0, z);
                tran.SetParent(Parent);
            }
        }
    }
}
#endif
