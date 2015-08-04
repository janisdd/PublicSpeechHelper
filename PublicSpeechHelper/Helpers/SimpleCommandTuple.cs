using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper.Helpers
{
    public class SimpleCommandTuple
    {


        public Action Action { get; set; }

        public bool IsEnabled { get; set; }


        public SimpleCommandTuple(Action action, bool isEnabled)
        {

            Action = action;
            IsEnabled = isEnabled;
        }
    }
}
