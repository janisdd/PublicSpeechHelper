using System;
using System.Reflection;

namespace PublicSpeechHelper.SpeechApi
{
    public class SpeechParameterConverter
    {
        /// <summary>
        /// the converter key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// the method info to invoke the method
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// the instance to invoke the parameter or null for static methods
        /// </summary>
        public object InvokingInstance { get; set; }

        /// <summary>
        /// the type to create the invoking instance
        /// </summary>
        public Type ExecutingType { get; set; }
    }
}
