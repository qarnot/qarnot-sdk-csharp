using System;

namespace qarnotsdk
{
    public class Error
    {
        public string Message { get; set; }

        public Error (string msg) {
            Message = msg;
        }

        public Error () {
        }
    }
}

