using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PublicSpeechHelper.SpeechApi;

namespace PublicSpeechHelper.Helpers
{
    public class SpeechParameterStream
    {
        /// <summary>
        /// the current command
        /// </summary>
        public SpeechTuple SpeechTuple { get; set; }

        /// <summary>
        /// the current parameter info
        /// </summary>
        public SpeechParameterInfo SpeechParameterInfo { get; set; }

        /// <summary>
        /// the text the parameter was recognized
        /// </summary>
        public string RecognizedText { get; set; }

        public SpeechParameterStream(string recognizedText, SpeechTuple speechTuple, SpeechParameterInfo speechParameterInfo)
        {
            this.RecognizedText = recognizedText;
            this.SpeechTuple = speechTuple;
            this.SpeechParameterInfo = speechParameterInfo;
        }
    }
}
