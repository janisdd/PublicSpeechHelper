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
        /// the text with the parameter was recognized
        /// </summary>
        public string RecognizedParameterNameText { get; set; }

        public SpeechParameterStream(string recognizedParameterNameText, SpeechTuple speechTuple, SpeechParameterInfo speechParameterInfo)
        {
            this.RecognizedParameterNameText = recognizedParameterNameText;
            this.SpeechTuple = speechTuple;
            this.SpeechParameterInfo = speechParameterInfo;
        }
    }
}
