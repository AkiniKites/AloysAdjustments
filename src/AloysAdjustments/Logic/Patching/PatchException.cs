using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Logic.Patching
{
    public class PatchException : Exception
    {
        public PatchException() { }

        public PatchException(string message) 
            : base(message) { }

        public PatchException(string message, Exception innerException) 
            : base(message, innerException) { }

        protected PatchException(SerializationInfo info, StreamingContext context) 
            : base(info, context) { }
    }
}
