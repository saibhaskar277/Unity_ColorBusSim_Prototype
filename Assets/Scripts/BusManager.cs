using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.UI;

public class BusManager : MonoBehaviour 
{
    public static BusManager Instance;


    [SerializeField] SplineContainer currentPath;
    [SerializeField] SplineContainer endPath;

    [SerializeField]BusDataContainer currentBus;
    [SerializeField] Transform busSpawnPoint;
    [SerializeField] List<PassengerStop> passengerStops = new List<PassengerStop>();

    public event Action<List<PassengerType>> OnBusStopChanged;
    List<PassengerType> availableBusStops = new List<PassengerType>();

    List<BaseBus> baseBuses = new List<BaseBus>();

    //public event Action onEndGame;

    [SerializeField] Button restartBtn;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        restartBtn.onClick.AddListener(RestartGame);

        StartGame();
    }

    public void Shuffle(List<PassengerObject> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            PassengerObject temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
        //onEndGame.Invoke();
        /*foreach (var item in baseBuses)
        {
            Destroy(item.gameObject);
        }
        baseBuses.Clear();
        StartGame();*/
    }

    public void StartGame()
    {
        float busOffset = 1.3f;
        int index = 0;
        List<PassengerObject> passengers = new List<PassengerObject>();
        foreach (var item in currentBus.data)
        {
            var obj = Instantiate(item.busPrefab.GetComponent<BaseBus>());
            obj.transform.position = busSpawnPoint.position;
            baseBuses.Add(obj);
            for (int i = 0; i < item.maxPassengerCount; i++)
            {
                var pasObj = Instantiate(item.passengerPrefab);
                pasObj.SetPassengerType(item.Type);
                passengers.Add(pasObj);
            }
            obj.SetData(currentPath, item.Type, item.maxPassengerCount,endPath);
            obj.transform.position += Vector3.right * (index * busOffset);
            index++;
        }

        Shuffle(passengers);

        int passengerIndex = 0;
        Dictionary<PassengerType,Material> materialMap = new Dictionary<PassengerType,Material>();
        foreach (var item in baseBuses)
        {
            materialMap.Add(item.busType,currentBus.GetMaterialByType(item.busType));
            for (int i = 0; i < 3; i++)
            {
                item.SetPassengers(passengers[passengerIndex]);
                var passengerPosList = item.StartWithPassengers();
                passengers[passengerIndex].MovePassenger(passengerPosList[i]);
                passengerIndex++;
            }
        }

        foreach (var item in passengerStops)
        {
            item.ResetPassengerStop(materialMap);
            item.onBusTypeChanged += onBusTypeChangedListner;
        }
    }

    private void onBusTypeChangedListner(PassengerType type)
    {
        if (!availableBusStops.Contains(type))
        {
            availableBusStops.Add(type);
        }
        else
        {
            availableBusStops.Remove(type);
        }
        OnBusStopChanged.Invoke(availableBusStops);
    }

    private void Update()
    {
        if (baseBuses.Count<=0)
            return;

        foreach (var item in baseBuses)
        {
            item.UpdateBusPos();
        }
    }
}


