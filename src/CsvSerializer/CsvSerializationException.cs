using System;
using System.Collections.Generic;
using System.Text;

namespace CsvSerializer
{
    public class CsvSerializationException : Exception
    {
        public CsvSerializationException(string message) : base(message)
        {
        }

        public CsvSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
