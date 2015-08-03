using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PublicSpeechMethodAttribute : Attribute
    {
        /// <summary>
        /// the language for the method name
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// the method name the user needs to call
        /// </summary>
        public List<string> SpeechNames { get; set; }


        public PublicSpeechMethodAttribute(string lang, params string[] speechName)
        {
            this.Lang = lang;
            this.SpeechNames = speechName.ToList();
        }
    }
}
