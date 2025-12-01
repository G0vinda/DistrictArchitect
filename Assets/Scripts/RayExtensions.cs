using System;
using UnityEngine;

public static class RayExtensions
{

    public static bool TryGetPositionAtY(this Ray ray, float yValue, out Vector3 point)
    {
        point = Vector3.zero;
        if (ray.origin.y > yValue && ray.direction.y > 0 || ray.origin.y < yValue && ray.direction.y < 0)
            return false;

        var yDistanceToValue = yValue - ray.origin.y;
        var unitStepsNeeded = yDistanceToValue / ray.direction.y;
        point = ray.origin + ray.direction * unitStepsNeeded;
        return true;
    }
}