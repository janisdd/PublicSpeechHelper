using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper
{
    [AttributeUsage(AttributeTargets.Parameter,  AllowMultiple = true)]
    public class PublicSpeechArgumentAttribute : Attribute
    {
        /// <summary>
        /// the language for the method name
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// the method name the user needs to call
        /// </summary>
        public string SpeechName { get; set; }

        public PublicSpeechArgumentAttribute(string lang, string speechName)
        {
            this.Lang = lang;
            this.SpeechName = speechName;
        }
    }
}
