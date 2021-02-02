using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Utility
{
    public class AsyncException : Exception
    {
        public AsyncException(Exception innerException) 
            : base(innerException.Message, innerException) { }
    }
}
