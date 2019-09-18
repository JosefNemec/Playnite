using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices
{
    public class GenericResponse
    {
        public string Error
        {
            get; set;
        }

        public object Data
        {
            get; set;
        }

        public GenericResponse()
        {
        }

        public GenericResponse(object data)
        {
            Data = data;
        }
    }

    public class ErrorResponse : GenericResponse
    {
        public ErrorResponse(string error)
        {
            Error = error;
        }

        public ErrorResponse(Exception error)
        {
            Error = error.Message;
        }
    }

    public class ServicesResponse<T> : GenericResponse
    {
        public new T Data
        {
            get; set;
        }

        public ServicesResponse(T data)
        {
            Data = data;
        }
    }
}
