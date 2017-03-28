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

        public GenericResponse(object data, string error)
        {
            Data = data;
            Error = error;
        }
    }

    public class ServicesResponse<T> : GenericResponse
    {
        public new T Data
        {
            get; set;
        }

        public ServicesResponse(T data, string error)
        {
            Data = data;
            Error = error;
        }
    }
}
