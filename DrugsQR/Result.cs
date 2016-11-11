using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DrugsQR
{
    [DataContract] 
    public class Result
    {
        [DataMember]
        public object data {get;set;}
        [DataMember]
        public String statusText { get; set; }
        [DataMember]
        public IDictionary<String, String> extData { get; set; }
        [DataMember]
        public bool sucess { get; set; }
        [DataMember]
        public bool failed { get; set; }
        


    }
}
