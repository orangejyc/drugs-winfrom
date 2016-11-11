using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DrugsQR
{
    [DataContract] 
    public class LoginCmd
    {
        [DataMember]
        public string action { get; set; }

        [DataMember]
        public string uname { get; set; }

        [DataMember]
        public string pwd { get; set; }
    }
}
