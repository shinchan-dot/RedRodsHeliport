#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BuildingScript : MonoBehaviour
{
    public GameObject BuildingPrefab;
    public Transform Parent;
    public string BuildingName;
    public float BlockXStart;
    public float BlockXEnd;
    public float BlockZStart;
    public float BlockZEnd;
    public bool Warehouse = false;

    [ContextMenu("PUT_BUILDINGS")]
    public void PutBuildings()
    {
        for (float z = BlockZStart; z <= BlockZEnd; z += (100 + 30))
        {
            for (float x = BlockXStart; x <= BlockXEnd; x += (140 + 30))
            {
                PutBlock(x, z);
            }
        }
    }

    public void PutBlock(float positionX, float positionZ)
    {
        GameObject block = new GameObject("Block");
        block.transform.SetParent(Parent);
        block.transform.position = new Vector3(positionX, 0, positionZ);

        if (Warehouse)
        {
            PutWarehouse(0, 30, block.transform);
            PutWarehouse(0, -30, block.transform);
        }
        else
        {
            PutSubBlock(-38, -29, block.transform);
            PutSubBlock(38, -29, block.transform);
            PutSubBlock(-38, 29, block.transform);
            PutSubBlock(38, 29, block.transform);
        }
    }

    public void PutSubBlock(float positionX, float positionZ, Transform parent)
    {
        GameObject sub = new GameObject("SubBlock");
        sub.transform.SetParent(parent);
        sub.transform.localPosition = new Vector3(positionX, 0, positionZ);


        for (int rowPositionZ = -11; rowPositionZ <= 11; rowPositionZ += 22)
        {
            int r = Random.Range(0, 100);
            if (r < 20){PutRowInSubBlock(20, 20, 20, rowPositionZ, sub.transform);}
            else if (r < 30){PutRowInSubBlock(30, 20, 10, rowPositionZ, sub.transform);}
            else if (r < 40){PutRowInSubBlock(30, 10, 20, rowPositionZ, sub.transform);}
            else if (r < 50){PutRowInSubBlock(20, 30, 10, rowPositionZ, sub.transform);}
            else if (r < 60){PutRowInSubBlock(20, 10, 30, rowPositionZ, sub.transform);}
            else if (r < 65){PutRowInSubBlock(10, 30, 20, rowPositionZ, sub.transform);}
            else if (r < 70){PutRowInSubBlock(10, 20, 30, rowPositionZ, sub.transform);}
            else if (r < 80) { PutRowInSubBlock(30, 30, 0, rowPositionZ, sub.transform); }
            else if (r < 90) { PutRowInSubBlock(20, 40, 0, rowPositionZ, sub.transform); }
            else { PutRowInSubBlock(40, 20, 0, rowPositionZ, sub.transform); }
        }

    }

    public void PutRowInSubBlock(float scaleX1, float scaleX2, float scaleX3, float positionZ, Transform parent)
    {
        float space = 2;
        if (scaleX3 <= 0)
        {
            space = 4;
        }

            float positionX1 = -32 + (scaleX1 / 2);
        PutBuilding(scaleX1, positionX1, positionZ, parent);


        float positionX2 = positionX1 + (scaleX1 / 2) + space + (scaleX2 / 2);
        PutBuilding(scaleX2, positionX2, positionZ, parent);

        if (scaleX3 > 0)
        {
            float positionX3 = positionX2 + (scaleX2 / 2) + space + (scaleX3 / 2);
            PutBuilding(scaleX3, positionX3, positionZ, parent);
        }
    }

    private void PutBuilding(float scaleX, float positionX, float positionZ, Transform parent)
    {
        float height = RandomHeight();

        GameObject building = PrefabUtility.InstantiatePrefab(BuildingPrefab) as GameObject;
        building.name = BuildingName;
        building.transform.SetParent(parent);
        building.transform.localScale = new Vector3(scaleX, height, 20);
        building.transform.localPosition = new Vector3(positionX, height / 2, positionZ);
    }

    private float RandomHeight()
    {
        float height = 0;
        int r = Random.Range(0, 100);

        if (r < 30) {height = 30f;}
        else if (r < 50){height = 27.5f;}
        else if (r < 70){height = 25f;}
        else if (r < 80){height = 22.5f;}
        else if (r < 90){height = 20f;}
        else if (r < 95){height = 17.5f;}
        else { height = 15f; }

        return height;
    }

    private void PutWarehouse(float x, float z, Transform parent)
    {
        float scaleX;
        int r = Random.Range(0, 100);
        if (r < 60){scaleX = 140;}
        else if (r < 80){scaleX = 120;}
        else {scaleX = 100;}

        float positionOffsetX;
        r = Random.Range(0, 2);
        if (r < 1) { positionOffsetX = (140 - scaleX) / 2; }
        else { positionOffsetX = (140 - scaleX) / 2 * -1; }

        //
        float scaleZ;
        r = Random.Range(0, 100);
        if (r < 80){scaleZ = 40;}
        else {scaleZ = 30;}

        float positionOffsetZ = 0;
        r = Random.Range(0, 2);
        if (r < 1) { positionOffsetZ = (40 - scaleZ) / 2; }
        else { positionOffsetZ = (40 - scaleZ) / 2 * -1; }

        //
        float scaleY;
        r = Random.Range(0, 100);
        if (r < 60){scaleY = 10f;}
        else if (r < 80){scaleY = 7.5f;}
        else {scaleY = 5f;}

        GameObject building = PrefabUtility.InstantiatePrefab(BuildingPrefab) as GameObject;
        building.name = BuildingName;
        building.transform.SetParent(parent);
        building.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        building.transform.localPosition = new Vector3(x + positionOffsetX, scaleY / 2, z + positionOffsetZ);
    }
}
#endif
