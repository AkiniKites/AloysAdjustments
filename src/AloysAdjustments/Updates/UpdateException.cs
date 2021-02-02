using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Updates
{
    public class UpdateException : Exception
    {
        public UpdateException() { }

        public UpdateException(string message) 
            : base(message) { }

        public UpdateException(string message, Exception innerException) 
            : base(message, innerException) { }

        protected UpdateException(SerializationInfo info, StreamingContext context) 
            : base(info, context) { }
    }
}
