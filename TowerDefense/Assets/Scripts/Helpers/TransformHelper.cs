using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformHelper
{
    #region Class definitions

    public class Walker : IEnumerable
    {
        public Transform current;
        private bool startFromSource;

        /// <summary>
        /// Walks recursively through the hierarchy from provided point.
        /// </summary>
        /// <param name="transform">Starting node in the hierarchy</param>
        /// <param name="includeSource">If true, the first node is the starting node</param>
        public Walker(Transform transform, bool includeSource = false)
        {
            current = transform;
            startFromSource = includeSource;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public WalkerEnum GetEnumerator()
        {
            return new WalkerEnum(this, startFromSource);
        }
    }

    public class WalkerEnum : IEnumerator
    {
        private Transform parent;
        private Transform current;

        private int position;
        private bool returnParent;

        private Walker walker;
        private WalkerEnum child;

        //private string path;

        /// <summary>
        /// Walks recursively through the children of the current node in provided walker.
        /// Creates internal instances for each child that has children of it's own. 
        /// </summary>
        /// <param name="walker">The walker enumerable that started the process</param>
        /// <param name="returnParentFirst">If true, returns the parent node first</param>
        public WalkerEnum(Walker walker, bool returnParentFirst)
        {
            this.walker = walker;

            position = -1;
            current = null;
            parent = walker.current;
            returnParent = returnParentFirst;

            //Debug.Log("Starting to walk " + parent.name);
        }

        public bool MoveNext()
        {
            if (returnParent)
                return true;

            // Walk through the grandchildren first
            if (child != null)
            {
                if (child.MoveNext())
                    return true;

                //Debug.Log("Moving forward from walker path " + current.name + "[" + position + "]/" + walker.current.name);
                current = null;
                child = null;
            }

            // Move into the previous child if it his children of it's own
            if (current != null && current.childCount > 0)
            {
                //Debug.Log("Previous node " + current.name + " has " + current.childCount + " children!");
                child = new WalkerEnum(walker, false);
                return child.MoveNext();
            }

            position++;

            if (position >= parent.childCount)
                return false;

            current = parent.GetChild(position);
            walker.current = current;

            return true;
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public Transform Current
        {
            get
            {
                try
                {
                    if (returnParent)
                    {
                        returnParent = false;
                        return parent;
                    }

                    // Only allow returning from the walker if it has initialized (the current child has been returned first)
                    if (child != null && child.position >= 0)
                        return child.Current;

                    //Debug.Log("Current node: " + parent.name + "[" + position + "]/" + current.name);
                    return current;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// Sets the provided Transform object to the target.
    /// </summary>
    /// <param name="transform">Object to align</param>
    /// <param name="target">Target to align to</param>
    /// <param name="matchScale">If false, scale is left untouched</param>
    public static void Align(Transform transform, Transform target, bool matchScale = false)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;

        if (matchScale)
            transform.localScale = target.localScale;
    }

    /// <summary>
    /// Sets the target Transform under the specified parent.
    /// Also optionally resets target transform parameters as a child.
    /// </summary>
    /// <param name="target">Target object</param>
    /// <param name="parent">Target's new parent</param>
    /// <param name="resetLocalPosition">If true, sets target local position as Vector3.zero</param>
    /// <param name="resetLocalRotation">If true, sets target local rotation as Quaternion.identity</param>
    /// <param name="resetLocalScale">If true, sets target local scale as Vector3.one</param>
    public static void SetParent(Transform target, Transform parent, bool resetLocalPosition=true, bool resetLocalRotation = true, bool resetLocalScale = true)
    {
        target.SetParent(parent);
        if (resetLocalPosition) target.localPosition = Vector3.zero;
        if (resetLocalRotation) target.localRotation = Quaternion.identity;
        if (resetLocalScale) target.localScale = Vector3.one;
    }

    /// <summary>
    /// Sets the target Transform under the specified parent.
    /// Also optionally resets target transform parameters as a child.
    /// </summary>
    /// <param name="target">Target object</param>
    /// <param name="parent">Target's new parent</param>
    /// <param name="worldPositionStays">If false, UI RectTransforms behave better when parenting</param>
    /// <param name="resetLocalPosition">If true, sets target local position as Vector3.zero</param>
    /// <param name="resetLocalRotation">If true, sets target local rotation as Quaternion.identity</param>
    /// <param name="resetLocalScale">If true, sets target local scale as Vector3.one</param>
    public static void SetParent(RectTransform target, Transform parent, bool worldPositionStays = true, bool resetLocalPosition = true, bool resetLocalRotation = true, bool resetLocalScale = true)
    {
        target.SetParent(parent, worldPositionStays);
        if (resetLocalPosition) target.localPosition = Vector3.zero;
        if (resetLocalRotation) target.localRotation = Quaternion.identity;
        if (resetLocalScale) target.localScale = Vector3.one;
    }

    /// <summary>
    /// Creates a parent for target child Transform and parents the child under the new Transform.
    /// Also optionally sets the new parent's position, rotation and scale
    /// </summary>
    /// <param name="child">Target object</param>
    /// <param name="name">New object's name</param>
    /// <param name="inheritParent">New object will be placed under the same parent as the child currently is</param>
    /// <param name="inheritLocalPosition">If true, the parent position will be the child's current position</param>
    /// <param name="inheritLocalRotation">If true, the parent rotation will be the child's current rotation</param>
    /// <param name="inheritLocalScale">If true, the parent scale will be the child's current scale</param>
    /// <returns>The new parent object</returns>
    public static Transform CreateParent(Transform child, string name, bool inheritParent = true, bool inheritLocalPosition = true, bool inheritLocalRotation = true, bool inheritLocalScale = true)
    {
        Transform parent = new GameObject(name).transform;

        if (inheritLocalPosition | inheritLocalRotation | inheritLocalScale)
            SetParent(parent, child, inheritLocalPosition, inheritLocalRotation, inheritLocalScale);

        parent.SetParent(inheritParent ? child.parent : null);
        child.parent = parent;

        return parent;
    }

    /// <summary>
    /// Creates an empty object as child of the provided parent, inheriting the parent's Transform.
    /// </summary>
    /// <param name="parent">The parent for the the object</param>
    /// <param name="name">New object's name</param>
    /// <returns>Thre new child object</returns>
    public static Transform CreateChild(Transform parent, string name)
    {
        Transform child = new GameObject(name).transform;
        SetParent(child, parent, true, true, true);
        return child;
    }

    /// <summary>
    /// Snaps the target object to another via matching the position and rotation of the mounting point.
    /// </summary>
    /// <param name="target">Object to position and rotate</param>
    /// <param name="mountingPoint">The object to match the position and rotation with</param>
    /// <param name="attachPoint">The object to snap to</param>
    public static void SnapToByMountingPoint(Transform target, Transform mountingPoint, Transform attachPoint)
    {
        target.rotation = attachPoint.rotation * (Quaternion.Inverse(mountingPoint.rotation) * target.rotation);
        target.position = attachPoint.position - (mountingPoint.position - target.position);
    }
    
    /// <summary>
    /// Sets the layer of all objects in a Transform hierarchy, starting from the provided object.
    /// </summary>
    public static void SetLayerInHierarchy(Transform transform, int layer)
    {
        foreach (Transform node in new Walker(transform, true))
            node.gameObject.layer = layer;
    }

    public static string GetPathInHierarchy(Transform transform)
    {
        string name = transform.name;
        while (transform.parent != null)
        {
            name = string.Concat(transform.name, "/", name);
            transform = transform.parent;
        }

        return name;
    }
}
