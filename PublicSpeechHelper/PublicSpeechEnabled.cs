﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublicSpeechHelper
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PublicSpeechEnabled : Attribute
    {
    }
}