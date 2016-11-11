using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DrugsQR
{
    public class BootParameter
    {

        private string host;  
        public string Host
        {  
            get  
            {
                return (string.Empty != host||null==host) ? host : "118.26.131.19";  
            }
            set
            {
                this.host = value;
            }
        }

        private int port;
        public int Port
        {
            get
            {
                return 0 != port ? port : 8885;
            }
            set
            {
                this.port = value;
            }
        }   
    }
}
