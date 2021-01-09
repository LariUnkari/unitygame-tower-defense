using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialSearch
{
    public struct Result<T>
    {
        public T item;
        public float distance;
        public int totalObjectsInRange;

        public static Result<T> Empty()
        {
            Result<T> result = new Result<T>();
            result.item = default(T);
            result.distance = float.MaxValue;
            result.totalObjectsInRange = 0;
            return result;
        }
    }

    public delegate Transform GetTransform<T>(T element);

    public static Result<T> FindClosest<T>(Vector3 position, float range, ICollection<T> collection, GetTransform<T> getTransformFunction)
    {
        Result<T> result = Result<T>.Empty();
        float distSqr, rangeSqr = range * range;
        Vector3 vector;

        foreach (T element in collection)
        {
            vector = getTransformFunction(element).position - position;
            distSqr = vector.sqrMagnitude;

            if (distSqr <= rangeSqr)
            {
                if (distSqr < result.distance)
                {
                    result.item = element;
                    result.distance = distSqr;
                }
            }
        }

        result.distance = Mathf.Sqrt(result.distance);
        return result;
    }
}
