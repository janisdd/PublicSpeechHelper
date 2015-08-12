using System.Collections.Generic;
using System.Reflection;
using PublicSpeechHelper.Helpers;

namespace PublicSpeechHelper.SpeechApi
{

    /// <summary>
    /// a crawled speech enabled parameter
    /// </summary>
    public class SpeechParameter
    {
        /// <summary>
        /// the speech synonyms for this parameter
        /// </summary>
        public List<string> SpeechNames { get; set; }

        /// <summary>
        /// the real parameter info
        /// </summary>
        public ParameterInfo ParameterInfo { get; set; }


        /// <summary>
        /// the converter for this parameter
        /// </summary>
        public SpeechParameterConverter Converter { get; set; }
    }
}
