using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PublicSpeechHelper.SpeechApi;

namespace PublicSpeechHelper.Helpers
{
    public class SpeechParameterInfo
    {
        public SpeechParameter Parameter { get; set; }

        public object Value { get; set; }

        public SpeechParameterInfo(SpeechParameter parameter)
        {
            this.Parameter = parameter;
        }
    }
}
