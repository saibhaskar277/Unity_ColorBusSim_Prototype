using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerStop : MonoBehaviour
{
    public PassengerType type {  get; private set; }
    public int passengerCount {  get; private set; }

    List<PassengerObject> passengerObjects = new List<PassengerObject>();
    public Action<PassengerType> onBusTypeChanged;

    Dictionary<PassengerType,Material> materials = new Dictionary<PassengerType,Material>();
    Material defaultMaterial;
    public void ResetPassengerStop(Dictionary<PassengerType, Material> materials)
    {
        type = PassengerType.None;
        passengerCount = 0;
        this.materials = materials;
    }


    IEnumerator WaitForManager()
    {
        yield return new WaitUntil(() => BusManager.Instance != null);
        //BusManager.Instance.onEndGame += onGameEnded;
    }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();  
        defaultMaterial = meshRenderer.material;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForManager());
    }

    private void onGameEnded()
    {
        foreach (var item in passengerObjects)
        {
            Destroy(item.gameObject);
        }
        passengerObjects.Clear();
    }

    MeshRenderer meshRenderer;
   
    public void SetPassengerStopType(PassengerType type)
    {
        if (this.type == PassengerType.None)
        {
            this.type = type;
            meshRenderer.material = materials[type];
            onBusTypeChanged.Invoke(type);
            
        }
    }

    public void PassengersGetIn(PassengerObject passenger)
    {
        passengerObjects.Add(passenger);
        passenger.MovePassenger(transform);
        passengerCount++;
    }

    List<PassengerObject> passengers = new List<PassengerObject>();
    public List<PassengerObject> BusPickUp(int count)
    {
        passengers.Clear();
        for (int i = 0; i < count; i++)
        {
            passengers.Add(passengerObjects[i]);
        }

        passengerObjects.RemoveRange(0,count);
        
        passengerCount -=count;
        if (passengerCount <= 0)
        {
            meshRenderer .material = defaultMaterial;
            onBusTypeChanged.Invoke(type);
            type = PassengerType.None;
        }

        return passengers;
    }

    private void OnDisable()
    {
        //BusManager.Instance.onEndGame -= onGameEnded;
    }

}
