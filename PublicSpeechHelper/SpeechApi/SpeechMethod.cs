using System;
using System.Collections.Generic;
using System.Reflection;

namespace PublicSpeechHelper.SpeechApi
{

    /// <summary>
    /// a crawled speech enabled method
    /// </summary>
    public class SpeechMethod
    {
        /// <summary>
        /// a key for code access
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// a key for grouping multiple methods
        /// </summary>
        public string SpeechGroupKey { get; set; }

        /// <summary>
        /// the language (culture name)
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// all the speech synonyms
        /// </summary>
        public List<string> SpeechNames { get; set; }

        /// <summary>
        /// the list of arguments for this method
        /// </summary>
        public List<SpeechParameter> Arguments { get; set; }

        /// <summary>
        /// the real method info
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// the class needed to execute the method or null for static methods
        /// </summary>
        public Type ExecutingType { get; set; }

        /// <summary>
        /// creates a new SpeechMethod
        /// </summary>
        public SpeechMethod()
        {
            Arguments = new List<SpeechParameter>();
            SpeechNames = new List<string>();
            this.Key = "";
            this.SpeechGroupKey = "";
        }

    }
}
