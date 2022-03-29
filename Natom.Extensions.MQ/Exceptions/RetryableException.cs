using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natom.Extensions.MQ.Exceptions
{
    public class RetryableException : Exception
    {
        private readonly Exception _exception;
        private readonly int _delayMiliseconds;

        public Exception GetException() => _exception;
        public int GetDelayMiliseconds() => _delayMiliseconds;

        public RetryableException(Exception exception, int delayMiliseconds = 500)
        {
            _exception = exception;
            _delayMiliseconds = delayMiliseconds;
        }
    }
}
