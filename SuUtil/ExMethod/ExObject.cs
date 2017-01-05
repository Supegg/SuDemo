using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuUtil.ExMethod
{
    public static class ExObject
    {
        public static bool IsNull(this object o)
        {
            return o == null;
        }
    }
}
