using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IvleLapiPortableClient.Exceptions
{
    class ModelParseJsonException : Exception
    {
        private static const String ERROR_MESSAGE =
            "Exception: Invalid Json string.";
        public ModelParseJsonException()
            : base(ERROR_MESSAGE)
        {
        }
    }
}
