using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper.Helpers
{
    public class SpeechStream
    {
        /// <summary>
        /// the current command
        /// </summary>
        public SpeechTuple SpeechTuple { get; set; }

        /// <summary>
        /// the current parameter info
        /// </summary>
        public List<SpeechParameterStream> SpeechParameterStreams { get; set; }

        /// <summary>
        /// the text the method was recognized
        /// </summary>
        public string RecognizedText { get; set; }

        public SpeechStream(string recognizedText, SpeechTuple speechTuple, params SpeechParameterStream[] speechParameterStreams)
        {
            this.RecognizedText = recognizedText;
            this.SpeechTuple = speechTuple;
            this.SpeechParameterStreams = speechParameterStreams.ToList();
        }
    }
}
