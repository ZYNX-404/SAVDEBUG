using UnityEngine;

public class TaxiPath : MonoBehaviour
{
    public Transform[] taxiPoints;

    public Vector3 GetPoint(int i)
    {
        if (taxiPoints == null || taxiPoints.Length == 0)
            return transform.position;

        i = Mathf.Clamp(i, 0, taxiPoints.Length - 1);
        return taxiPoints[i].position;
    }

    public int Count
    {
        get
        {
            if (taxiPoints == null)
                return 0;

            return taxiPoints.Length;
        }
    }
}