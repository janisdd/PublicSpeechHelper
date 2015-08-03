using System;
using System.Collections.Generic;
using System.Linq;

namespace PublicSpeechHelper.SpeechApi
{

    /// <summary>
    /// marks a method to be speech enabled
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SpeechMethodAttribute : Attribute
    {
        /// <summary>
        /// the language (e.g. de-de) for the method (needs to be a valid culture name)
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// all speech synonyms for the method
        /// </summary>
        public List<string> SpeechNames { get; set; }

        /// <summary>
        /// a key for code access
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// a key for grouping multiple methods
        /// </summary>
        public string SpeechGroupKey { get; set; }

        /// <summary>
        /// creates a new SpeechMethodAttribute
        /// </summary>
        /// <param name="lang">the culture name (e.g. de-de)</param>
        /// <param name="speechNames">all speech synonyms for the method</param>
        public SpeechMethodAttribute(string lang, params string[] speechNames)
        {
            this.Lang = lang;
            this.SpeechNames = speechNames.ToList();
            this.Key = "";
            this.SpeechGroupKey = "";
        }
    }
}
