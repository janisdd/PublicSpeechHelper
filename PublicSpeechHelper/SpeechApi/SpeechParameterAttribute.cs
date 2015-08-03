using System;
using System.Collections.Generic;
using System.Linq;

namespace PublicSpeechHelper.SpeechApi
{
    /// <summary>
    /// marks a parameter to be speech enabled
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter,  AllowMultiple = true)]
    public class SpeechParameterAttribute : Attribute
    {
        /// <summary>
        /// the language (e.g. de-de) for the method (needs to be a valid culture name)
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// the speech synonym parameter names
        /// </summary>
        public List<string> SpeechNames { get; set; }

        /// <summary>
        /// the key for code access
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// creates a new SpeechParameterAttribute
        /// </summary>
        /// <param name="lang">the culture name (e.g. de-de)</param>
        /// <param name="speechNames">the speech name</param>
        public SpeechParameterAttribute(string lang,params string[] speechNames)
        {
            this.Lang = lang;
            this.SpeechNames = speechNames.ToList();
        }
    }
}
