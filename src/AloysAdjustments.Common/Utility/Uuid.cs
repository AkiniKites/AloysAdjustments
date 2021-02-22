using System;
using System.Collections.Generic;
using System.Text;

namespace AloysAdjustments.Utility
{
    public class Uuid
    {
        public virtual Guid New()
        {
            return Guid.NewGuid();
        }
    }
}
