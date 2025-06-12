using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Exceptions
{
	//ah ssibal
    public class InfrastructureException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public InfrastructureException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = (int)statusCode;
        }
        public InfrastructureException(HttpStatusCode statusCode, string errorCode, string message) : base(message)
        {
            StatusCode = (int)statusCode;
            ErrorCode = errorCode;
        }
        public InfrastructureException(HttpStatusCode statusCode, string errorCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = (int)statusCode;
            ErrorCode = errorCode;
        }
    }
}
