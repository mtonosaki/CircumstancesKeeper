using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwatchTSWRLP
{
    public class SecretParameterBase
    {
        public virtual string UserName => "DigestAuthUserNameHere"; // Your WebCamera's User Name
        public virtual string Password => "DigestAuthPasswordHere"; // Your WebCamera's Password
        public virtual string Url => "http://192.168.1.1:47826";    // HTTP Server listener
    }

    /// <summary>
    /// Private secret key (Please override key texts for your secret)
    /// </summary>
    public partial class MySecretParameter : SecretParameterBase
    {
    }
}
