using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HZDOutfitEditor.Utility
{
    public class HzdException : Exception
    {
        public HzdException() { }

        public HzdException(string message) 
            : base(message) { }

        public HzdException(string message, Exception innerException) 
            : base(message, innerException) { }

        protected HzdException(SerializationInfo info, StreamingContext context) 
            : base(info, context) { }
    }
}
