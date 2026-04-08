using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BusDataContainer", menuName = "Game Data/Bus Data")]
public class BusDataContainer : ScriptableObject
{
    public List<BusData> data;

    public Material GetMaterialByType(PassengerType type)
    {
        return data.Find(x=>x.Type==type).busStopMaterial;
    }
}

[System.Serializable]
public class BusData
{
    public PassengerType Type;
    public Color32 color;
    public int maxPassengerCount;
    public BaseBus busPrefab;
    public PassengerObject passengerPrefab;
    public Material busStopMaterial;
}