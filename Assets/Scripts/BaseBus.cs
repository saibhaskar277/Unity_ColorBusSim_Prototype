using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

public class BaseBus : MonoBehaviour, IBus
{
    public int MaxPassengerCount { get; private set; }
    public PassengerType busType { get; private set; }

    public float BusPos { get; private set; }

    bool mustStop = false;

    private float currentSpeed = 0f;
    private float t = 0f;
    float splineLength;
    
    SplineContainer spline;
    SplineContainer endSpline;

    public float maxSpeed = 3f;
    public float accel = 3f;
    public float decel = 5f;

    bool startMoving = false;

    List<PassengerObject> passengerObjects = new List<PassengerObject>();
    [SerializeField] List<Transform> passengerTargetPos = new List<Transform>();

    private int currentCount = 3;

    public void SetData(SplineContainer spline, PassengerType type, int passengerMaxCount,SplineContainer endSpline)
    {
        MaxPassengerCount = passengerMaxCount;
        this.spline = spline;
        this.endSpline = endSpline;
        busType = type;
        splineLength = spline.CalculateLength();
        passengerObjects.Clear();
    }

    List<PassengerType>avalibleBusStops = new List<PassengerType>();

    private void OnEnable()
    {
        BusManager.Instance.OnBusStopChanged += OnBusStopChangedlistner;
    }

    private void OnBusStopChangedlistner(List<PassengerType>  obj)
    {
        avalibleBusStops = obj;
    }

    private void OnDisable()
    {
        BusManager.Instance.OnBusStopChanged -= OnBusStopChangedlistner;
    }

    public void SetPassengers(PassengerObject passenger)
    {
        if (passenger.PassengerType!=busType)
        {
            passengerObjects.Add(passenger);
        }
    }

    private void OnMouseDown()
    {
        startMoving = true;
        Handheld.Vibrate();
    }

    [SerializeField] LayerMask busMask;
    bool isHitTarget = false;

    public bool CheckForObstacle()
    {
        isHitTarget = false;
        if (Physics.Raycast(transform.position,transform.forward,1.5f,busMask))
        {
            isHitTarget = true;
        }

        return isHitTarget;
    }

    public List<PassengerObject> RemoveMostFrequent(List<PassengerObject> list)
    {
        List<PassengerObject> removedPassengers = new List<PassengerObject>();

        if (list == null || list.Count == 0)
            return removedPassengers;
        var mostFrequentType = list
            .GroupBy(x => x.PassengerType)
            .OrderByDescending(g => g.Count())
            .First().Key;

        removedPassengers = list
            .Where(x => x.PassengerType == mostFrequentType)
            .ToList();

        return removedPassengers;
    }

    [SerializeField] LayerMask busStopMask;
    RaycastHit stopHit;

    public Transform CheckForSpace()
    {
        Transform target = passengerTargetPos[0].transform;
        foreach (var item in passengerTargetPos)
        {
            if (item.transform.childCount==0)
            {
                target = item.transform;
                break;
            }
        }
        return target.transform;
    }


    public List<Transform> StartWithPassengers()
    {
        return passengerTargetPos;
    }

    bool passengerDrop = false;
    bool isBusFilled = false;

    async void StopCoolDown()
    {
        await Task.Delay(1000);
        passengerDrop = false;
    }
    
    public void CheckForBusStop()
    {
        if (passengerDrop)
            return;
        
        if (Physics.Raycast(transform.position, transform.forward, out stopHit, 1.5f, busStopMask))
        {
            
            StopCoolDown();
            var stopObj = stopHit.transform.GetComponent<PassengerStop>();
            if (stopObj == null)
                return;

            int stopCount = stopObj.passengerCount;
            PassengerType stopType = stopObj.type;

            if (stopType==PassengerType.None)
            {
                if (passengerObjects.Count>0)
                {
                    passengerDrop = true;
                    var frequentType = RemoveMostFrequent(passengerObjects);

                    if (avalibleBusStops.Contains(frequentType[0].PassengerType))
                        return;

                    if (frequentType.Count > 0)
                    {
                        foreach (var item in frequentType)
                        {
                            stopObj.PassengersGetIn(item);
                            passengerObjects.Remove(item);
                        }
                        stopObj.SetPassengerStopType(frequentType[0].PassengerType);
                        currentCount -= frequentType.Count;
                    }
                }
            }
            else
            {
                if (stopType == this.busType)
                {
                    int availableSpace = MaxPassengerCount - currentCount;
                    int passengersToPickUp = Mathf.Min(stopCount, availableSpace);

                    if (passengersToPickUp > 0)
                    {
                        passengerDrop = true;
                        var newPassengers = stopObj.BusPickUp(passengersToPickUp);

                        newPassengers.ForEach(x => {

                            x.MovePassenger(CheckForSpace());
                            Handheld.Vibrate();
                        });
                        currentCount += newPassengers.Count;
                    }

                    /*if (pickUpCoroutine != null)
                        StopCoroutine(pickUpCoroutine);

                    pickUpCoroutine = StartCoroutine(StartPickUp());*/
                }
                else
                {
                    var matchObjs = passengerObjects.Where(x => x.PassengerType == stopType).ToList();
                    if (matchObjs.Count > 0)
                    {
                        passengerDrop = true;
                        foreach (var obj in matchObjs)
                        {
                            stopObj.PassengersGetIn(obj);
                        }
                        currentCount -= matchObjs.Count;

                        passengerObjects.RemoveAll(x => x.PassengerType == stopType);
                    }
                }
            }

        }
    }

    Coroutine pickUpCoroutine;

    IEnumerator StartPickUp()
    {
        startMoving = false;
        yield return new WaitForSeconds(2);
        startMoving = true;
    }

    public void UpdateBusPos()
    {
        if (spline == null||!startMoving)
            return;

        mustStop = CheckForObstacle();
        CheckForBusStop();

        if (mustStop)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, decel * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, accel * Time.deltaTime);
        }

        if (splineLength > 0f)
        {
            t += (currentSpeed * Time.deltaTime) / splineLength;
            t = Mathf.Repeat(t, 1f);
            if (isBusFilled && t >= 0.97f)
            {
                t = 0f;
                spline = endSpline;
                startMoving = false;
            }
        }
       
        BusPos = t;

        transform.position = spline.EvaluatePosition(t);
        transform.rotation = Quaternion.LookRotation(
            spline.EvaluateTangent(t),
            Vector3.up
        );
    }
}
