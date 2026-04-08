using DG.Tweening;
using UnityEngine;

public class PassengerObject : MonoBehaviour
{
    public PassengerType PassengerType {  get; private set; }

    float animationDuration= 0.5f;


    public void SetPassengerType(PassengerType type)
    {
        PassengerType = type;
    }

    public void MovePassenger(Transform target)
    {
        transform.DOJump(target.position, 1, 1, animationDuration).SetEase(Ease.InSine).OnComplete(() =>
        {
            transform.SetParent(target);
        });
    }

}
