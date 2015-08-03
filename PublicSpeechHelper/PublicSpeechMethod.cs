using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PublicSpeechHelper
{
    public class PublicSpeechMethod
    {

        public string Lang { get; set; }

        public List<string> SpeechNames { get; set; }


        public List<PublicSpeechArgument> Arguments { get; set; }

        public MethodInfo Info { get; set; }

        public PublicSpeechMethod()
        {
            Arguments = new List<PublicSpeechArgument>();
            SpeechNames = new List<string>();
        }

    }
}
