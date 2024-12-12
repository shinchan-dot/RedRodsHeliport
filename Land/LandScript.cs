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
        int planeSize = 300 * 10;
        int planesPerSide = 10;
        int positionMax = planeSize * (planesPerSide / 2 - 1) + planeSize / 2;
        int positionMin = -positionMax;

        int count = 1;
        for (int z = positionMin; z <= positionMax; z += planeSize)
        {
            for (int x = positionMin; x <= positionMax; x += planeSize)
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
