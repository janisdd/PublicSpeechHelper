using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper.SpeechApi
{
    /// <summary>
    /// marks a method as a parameter converter method, the method must have 1 string parameter and the returning type must 
    /// be equal to the parameter type that uses this converter
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SpeechParameterConverterAttribute : Attribute
    {

        /// <summary>
        /// the converter key
        /// </summary>
        public string Key { get; set; }

        public SpeechParameterConverterAttribute(string key)
        {
            this.Key = key;
        }
    }
}
