using System;

namespace qarnotsdk
{
    public class Id
    {
        public Guid Guid { get; set; }
        public Id ()
        {
        }
        public Id(Guid guid)
        {
            Guid = guid;
        }
    }
}

