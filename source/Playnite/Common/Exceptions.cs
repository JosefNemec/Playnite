using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class NoDiskSpaceException : Exception
    {
        public long RequiredSpace { get; }

        public NoDiskSpaceException(long requiredSpace) : base()
        {
            RequiredSpace = requiredSpace;
        }

        public NoDiskSpaceException(string message) : base(message)
        {
        }

        public NoDiskSpaceException(string message, long requiredSpace) : base(message)
        {
            RequiredSpace = requiredSpace;
        }
    }
}
