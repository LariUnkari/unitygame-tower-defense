// A collection of helper functions for 3D vector mathematics
// Author: Lari Unkari

using UnityEngine;

public class Math3D
{

    #region Basic Geometry
    /// <summary>
    /// Returns the volume of a sphere of a specified radius.
    /// </summary>
    public static float SphereVolume(float radius)
    {
        return (4f / 3f) * Mathf.PI * Mathf.Pow(radius, 3f);
    }

    /// <summary>
    /// Returns the volume of a capsule primitive of a specified radius and cylindrical height.
    /// </summary>
    public static float CapsuleVolume(float radius, float cylindricalHeight)
    {
        if (cylindricalHeight > 0f)
            return Mathf.PI * radius * radius * ((4f / 3f) * radius + cylindricalHeight);

        return SphereVolume(radius);
    }

    #endregion
    #region Lines and points

    /// <summary>
    /// Returns the closest found position on line from point.
    /// </summary>
    public static Vector3 ClosestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        return origin + direction * Vector3.Dot(point - origin, direction);
    }

    /// <summary>
    /// Returns the closest found position on line from point
    /// </summary>
    public static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 posA, Vector3 posB)
    {
        Vector3 ab = posB - posA;
        Vector3 ap = point - posA;

        // Point is behind posA
        if (Vector3.Dot(ap, ab) <= 0f)
            return posA;

        Vector3 bp = point - posB;

        // Point is behind posB
        if (Vector3.Dot(bp, ab) >= 0f)
            return posB;

        // Point is perpendicular to segment, so closest position is within segment
        return Vector3.Cross(ab, ap);
    }

    /// <summary>
    /// Returns the shortest distance from point on segment
    /// </summary>
    public static float DistanceFromPointToSegment(Vector3 point, Vector3 posA, Vector3 posB)
    {
        Vector3 ab = posB - posA;
        Vector3 ap = point - posA;

        // Point is behind posA, return distance to posA
        if (Vector3.Dot(ap, ab) <= 0f)
            return ap.magnitude;

        Vector3 bp = point - posB;

        // Point is behind posB, return distance to posB
        if (Vector3.Dot(bp, ab) >= 0f)
            return bp.magnitude;

        // Point is perpendicular to the segment
        return (Vector3.Cross(ab, ap)).magnitude / ab.magnitude;
    }

    /// <summary>
    /// Returns true if the vector intersects the plane, and assigns the out parameter to the intersection point found
    /// </summary>
    public static bool VectorPlaneIntersect(Vector3 vectorPoint, Vector3 vectorLine, Vector3 planePoint, Vector3 planeNormal, out Vector3 intersection)
    {
        float dotN, dotD;
        Vector3 vector;

        dotN = Vector3.Dot((planePoint - vectorPoint), planeNormal);
        dotD = Vector3.Dot(vectorLine, planeNormal);

        if (!Mathf.Approximately(dotD, 0f))
        {
            vector = vectorLine.normalized * dotN / dotD;

            intersection = vectorPoint + vector;

            return true;
        }

        intersection = Vector3.zero;

        return false;
    }

    /// <summary>
    /// Returns the closest found position on the plane from point
    /// </summary>
    public static Vector3 ClosestPointOnPlane(Vector3 point, Vector3 planePosition, Vector3 planeNormal)
    {
        return point - planeNormal * Vector3.Dot(planeNormal, (point - planePosition));
    }

    /// <summary>
    /// Returns the shortest distance from point on plane
    /// </summary>
    public static float DistanceFromPointToPlane(Vector3 point, Vector3 planePosition, Vector3 planeNormal)
    {
        return Mathf.Abs(Vector3.Dot(planeNormal, (point - planePosition)));
    }

    /// <summary>
    /// Returns the vector projected onto the surface of a plane defined by it's normal
    /// </summary>
    public static Vector3 ProjectVectorOntoPlane(Vector3 vector, Vector3 planeNormal)
    {
        return vector - (planeNormal * Vector3.Dot(vector, planeNormal));
    }

    /// <summary>
    /// Returns positive values if intersection was found: 1 = intersection along line AB, 2 = line point A on plane, 3 = line point B on plane, 4 = line parallel to plane
    /// </summary>
    public static int LinePlaneIntersection(out Vector3 intersection, Vector3 linePointA, Vector3 linePointB, Vector3 planeNormal, Vector3 planePoint)
    {
        float dist, lengthSqr, dotA, dotB, dotDir;
        Vector3 vector = linePointB - linePointA;

        // Calculate the vector magnitude squared, for later comparison, and normalize the vector into a direction
        lengthSqr = vector.sqrMagnitude;
        vector = vector.normalized;

        // Calculate the dot product between the line direction and plane normal
        dotDir = Vector3.Dot(vector, planeNormal);

        // Calculate the dot product between the line from plane point to line point A and plane normal, same with B
        dotA = Vector3.Dot((planePoint - linePointA), planeNormal);
        dotB = Vector3.Dot((planePoint - linePointB), planeNormal);

        // If the vectors are not parallel, we can find an intersection...
        if (dotDir != 0f)
        {
            if (dotA == 0f)
            {
                intersection = linePointA;
                return 2;
            }
            if (dotB == 0f)
            {
                intersection = linePointB;
                return 3;
            }

            dist = dotA / dotDir;

            // Only accept positive distances within the specified limit, meaning we discard hits on the vector which are outside the line
            if (dist >= 0f && dist * dist <= lengthSqr)
            {
                // Calculate the intersection point
                intersection = linePointA + vector.normalized * dist;
                return 1;
            }
        }
        // ...unless the line overlaps the plane surface in which case we find infinite solutions
        else if (dotA == 0f)
        {
            // We can just report the end point
            intersection = linePointB;
            return 4;
        }

        // No intersection could be found
        intersection = Vector3.zero;
        return 0;
    }

    /// <summary>
    /// Try to find a position and time to intercept a moving target from a moving object with
    /// a projector-like interceptor while not accounting for any changes in trajectories
    /// </summary>
    /// <param name="startPosition">Start position</param>
    /// <param name="startVelocity">Start velocity</param>
    /// <param name="interceptSpeed">Speed to move interceptor with</param>
    /// <param name="targetPosition">Target position</param>
    /// <param name="targetVelocity">Target velocity</param>
    /// <param name="interceptPoint">Point of interception, if found</param>
    /// <param name="timeToTarget">Time to interception, if found</param>
    /// <returns>True if a solution was found</returns>
    public static bool TryGetInterception(Vector3 startPosition, Vector3 startVelocity, float interceptSpeed, Vector3 targetPosition, Vector3 targetVelocity, float minimumAccuracy, out Vector3 interceptPoint, out float timeToTarget)
    {
        Vector3 targetRelativePosition = targetPosition - startPosition;
        Vector3 targetRelativeVelocity = targetVelocity - startVelocity;

        if (TryGetInterceptTime(interceptSpeed, targetRelativePosition, targetRelativeVelocity, minimumAccuracy, out timeToTarget))
        {
            interceptPoint = targetPosition + timeToTarget * (targetRelativeVelocity);
            return true;
        }

        interceptPoint = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Try to calculate the time to intercept a target relative to the interceptor
    /// </summary>
    /// <param name="interceptSpeed">Speed to intercept with, like a projectile</param>
    /// <param name="targetRelativePosition">Target position</param>
    /// <param name="targetRelativeVelocity">Target velocity</param>
    /// <param name="timeToTarget">Time to interception</param>
    /// <returns>True if a solution was found</returns>
    public static bool TryGetInterceptTime(float interceptSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity, float minimumAccuracy, out float timeToTarget)
    {
        float velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if (velocitySquared < minimumAccuracy)
        {
            timeToTarget = 0f;
            return true;
        }

        float a = velocitySquared - interceptSpeed * interceptSpeed;

        // Handle similar velocities
        if (Mathf.Abs(a) < minimumAccuracy)
        {
            timeToTarget = Mathf.Max(-targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition)), 0f);
            return true;
        }

        float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
        float c = targetRelativePosition.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        {
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a);
            float t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            if (t1 > 0f)
                timeToTarget = t2 > 0f ? Mathf.Min(t1, t2) : t1;
            else
                timeToTarget = Mathf.Max(t2, 0f);

            return true;
        }
        else if (determinant < 0f)
        {
            timeToTarget = -1f;
            return false;
        }

        timeToTarget = Mathf.Max(-b / (2f * a), 0f);
        return true;
    }

    public static bool IsPointWithinBox(Vector3 point, Vector3 center, Vector3 size)
    {
        Vector3 offsetDoubled = (point - center) * 2f;
        return Mathf.Abs(offsetDoubled.x) <= Mathf.Abs(size.x) && Mathf.Abs(offsetDoubled.y) <= Mathf.Abs(size.y) && Mathf.Abs(offsetDoubled.z) <= Mathf.Abs(size.z);
    }

    public static bool IsPointWithinSphere(Vector3 point, Vector3 center, float radius)
    {
        return (point - center).sqrMagnitude <= radius * radius;
    }

    public static bool IsPointWithinCapsule(Vector3 point, Vector3 center, float radius, float cylindricalHeight)
    {
        Vector3 linePoint = ClosestPointOnLineSegment(point, center + Vector3.down * cylindricalHeight / 2f, center + Vector3.up * cylindricalHeight / 2f);
        return (point - linePoint).sqrMagnitude <= radius * radius;
    }

    #endregion
    #region Spherical

    /// <summary>
    /// Calculates a uniformly spread random vector within a cone defined by a direction and an angle.
    /// </summary>
    /// <param name="direction">The direction of the cone (tip to bottom)</param>
    /// <param name="angle">The angle from direction to the edge of the cone</param>
    /// <returns></returns>
    public static Vector3 RandomDirectionWithinCone(Vector3 direction, float angle)
    {
        if (angle <= 0f)
            return direction;

        Vector3 finalDirection = Vector3.zero;
        finalDirection.z = Random.Range(Mathf.Cos(angle), 1f);

        float phi = Random.Range(0f, 2f * Mathf.PI);
        float h = Mathf.Sqrt(1f - finalDirection.z * finalDirection.z);

        finalDirection.x = h * Mathf.Cos(phi);
        finalDirection.y = h * Mathf.Sin(phi);

        return Quaternion.FromToRotation(Vector3.forward, direction) * finalDirection;
    }

    #endregion
    #region Vertex shapes

    /// <summary>
    /// Returns an array of Vector3[4], the vertices of an arrow defined by given parameters. Order is: source, target, right head, left head.
    /// </summary>
    public static Vector3[] GetArrowVertices(Vector3 origin, Vector3 target, Vector3 normal, float arrowHeadLength = 0.25f, float arrowHeadAngle = 45f)
    {
        Vector3 direction = target - origin;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = origin;
        vertices[1] = target;
        vertices[2] = target + Quaternion.LookRotation(direction, normal) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * Vector3.forward * arrowHeadLength;
        vertices[3] = target + Quaternion.LookRotation(direction, normal) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * Vector3.forward * arrowHeadLength;

        return vertices;
    }

    /// <summary>
    /// Returns an array of Vector3[4], the vertices of a quad defined by the given parameters and centered at the position.
    /// </summary>
    public static Vector3[] GetQuadVertices(Vector3 position, Vector3 normal, Vector3 up, float width, float height)
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f * width, 0f, 0.5f * height),
            new Vector3(0.5f * width, 0f, 0.5f * height),
            new Vector3(0.5f * width, 0f, -0.5f * height),
            new Vector3(-0.5f * width, 0f, -0.5f * height)
        };

        // Rotate to match normal and up vectors
        Quaternion rot = Quaternion.LookRotation(up, normal);
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = position + rot * vertices[i];

        return vertices;
    }

    /// <summary>
    /// Returns an array of Vector3[8], the vertices of a cube defined by the given parameters with the bottom side centered at the position. Bottom side vertices are 0-3 and top side vertices 4-7.
    /// </summary>
    public static Vector3[] GetCubeVertices(Vector3 position, Vector3 forward, Vector3 up, float width, float height, float depth)
    {
        Vector3[] vertices = new Vector3[8];
        vertices[0] = new Vector3(-0.5f * width, 0f, 0.5f * depth);
        vertices[1] = new Vector3(0.5f * width, 0f, 0.5f * depth);
        vertices[2] = new Vector3(0.5f * width, 0f, -0.5f * depth);
        vertices[3] = new Vector3(-0.5f * width, 0f, -0.5f * depth);
        vertices[4] = vertices[0] + Vector3.up * height;
        vertices[5] = vertices[1] + Vector3.up * height;
        vertices[6] = vertices[2] + Vector3.up * height;
        vertices[7] = vertices[3] + Vector3.up * height;

        // Rotate to match normal and up vectors
        Quaternion rot = Quaternion.LookRotation(forward, up);
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = position + rot * vertices[i];

        return vertices;
    }

    /// <summary>
    /// Returns an array of Vector3[segments], the vertices of a circle defined by the given parameters
    /// </summary>
    public static Vector3[] GetCircleVertices(Vector3 position, Vector3 normal, Vector3 up, float radius, int segments)
    {
        Vector3[] vertices = new Vector3[segments];
        Quaternion rot = Quaternion.LookRotation(normal, up);
        Vector3 newVertex;
        float a, s1, c1;

        for (int i = 0; i < segments; i++)
        {
            a = ((float)i / segments) * 2f * Mathf.PI;
            s1 = Mathf.Sin(a);
            c1 = Mathf.Cos(a);

            newVertex.x = s1 * radius;
            newVertex.y = c1 * radius;
            newVertex.z = 0f;
            newVertex = position + rot * newVertex;

            vertices[i] = newVertex;
        }

        return vertices;
    }

    /// <summary>
    /// Returns an array of Vector3[segments], the vertices of an arc defined by the given parameters
    /// </summary>
    public static Vector3[] GetArcVertices(Vector3 position, Vector3 normal, Vector3 arcCenter, float radius, float radians, int segments)
    {
        Vector3[] vertices = new Vector3[segments + 1];
        Quaternion rot = Quaternion.LookRotation(normal, arcCenter);
        Vector3 newVertex;
        float a, s1, c1;

        for (int i = 0; i <= segments; i++)
        {
            a = 0.5f * Mathf.Lerp(-radians, radians, (float)i / segments);
            s1 = Mathf.Sin(a);
            c1 = Mathf.Cos(a);

            newVertex.x = s1 * radius;
            newVertex.y = c1 * radius;
            newVertex.z = 0f;
            newVertex = position + rot * newVertex;

            vertices[i] = newVertex;
        }

        return vertices;
    }

    #endregion
}
