using UnityEngine;

namespace DebugUtilities
{
    public class DBGLogger
    {
        public const string FORMAT_CALLER_NAME = "'{0}'";
        public const string FORMAT_CALLER_TYPE = "<{0}>";
        public const string FORMAT_TIMESTAMP_FRAME = "[{0}.{1}]";
        public const string FORMAT_TIMESTAMP_UTC = "[{0}]";

        public const string FORMAT_TIME = "yyyy-MM-dd HH:mm:ss.fff";

        public enum Element { Timestamp, Name, CallerType }
        public enum Mode { Timestamp = 1, TimestampName = 3, TimestampCallerType = 5, Everything = 7 };

        private static int s_lastLogFrame;
        private static int s_logCountFrame;

        public static void Log(string message, object callingObject, UnityEngine.Object context = null, int modeFlags = (int)Mode.TimestampCallerType)
        {
            UpdateLogCounter();
            Debug.Log(Format(message, callingObject, modeFlags), context);
        }

        public static void LogWarning(string message, object callingObject, UnityEngine.Object context = null, int modeFlags = (int)Mode.TimestampCallerType)
        {
            UpdateLogCounter();
            Debug.LogWarning(Format(message, callingObject, modeFlags), context);
        }

        public static void LogError(string message, object callingObject, UnityEngine.Object context = null, int modeFlags = (int)Mode.TimestampCallerType)
        {
            UpdateLogCounter();
            Debug.LogError(Format(message, callingObject, modeFlags), context);
        }

        public static string Format(string message, object callingObject, int modeFlags = (int)Mode.TimestampCallerType)
        {
            return GetTimeStamp() + " " + GetCallerName(callingObject, modeFlags) + GetCallerType(callingObject, modeFlags) + ": " + message;
        }

        private static string GetTimeStamp()
        {
            if (!Application.isEditor || Application.isPlaying)
                return string.Format(FORMAT_TIMESTAMP_FRAME, Time.frameCount, s_logCountFrame);

            return string.Format(FORMAT_TIMESTAMP_UTC, System.DateTime.UtcNow.ToString(FORMAT_TIME, System.Globalization.CultureInfo.InvariantCulture));
        }

        private static string GetCallerName(object callingObject, int modeFlags)
        {
            if (!IsModeElementSet(modeFlags, Element.Name))
                return "";

            if (callingObject == null)
                return "";

            // If caller is a type object (usually called from static context)
            if (callingObject is System.Type)
                return "";

            // If caller is of a Unity object type
            if (callingObject is UnityEngine.Object)
            {
                UnityEngine.Object uo = callingObject as UnityEngine.Object;
                if (uo != null)
                    return string.Format(FORMAT_CALLER_NAME, uo.name); // Object has an actual name

                // This handles objects that have been destroyed but are in memory
                return string.Format(FORMAT_CALLER_NAME, "");
            }

            // Caller is of an unhandled object type, return the type as name
            return string.Format(FORMAT_CALLER_NAME, string.Format(FORMAT_CALLER_TYPE, callingObject.GetType()));
        }

        private static string GetCallerType(object callingObject, int modeFlags)
        {
            if (!IsModeElementSet(modeFlags, Element.CallerType))
                return "";

            if (callingObject == null)
                return string.Format(FORMAT_CALLER_TYPE, "NULL");

            // If caller is a static type
            if (callingObject is System.Type)
                return string.Format(FORMAT_CALLER_TYPE, callingObject);

            // Caller is of a valid object type
            return string.Format(FORMAT_CALLER_TYPE, callingObject.GetType());
        }

        public static string GetObjectTypeName(object targetObject)
        {
            string name = targetObject.GetType().ToString();
            return name.Substring(name.LastIndexOf('.') + 1);
        }

        private static bool IsModeElementSet(int modeFlags, Element modeElement)
        {
            return (modeFlags & (1 << (int)modeElement)) > 0;
        }

        private static void UpdateLogCounter()
        {
            if (Application.isEditor && !Application.isPlaying)
                return;

            if (s_lastLogFrame < Time.frameCount)
            {
                s_lastLogFrame = Time.frameCount;
                s_logCountFrame = 0;
            }
            else
                s_logCountFrame++;
        }
    }
}