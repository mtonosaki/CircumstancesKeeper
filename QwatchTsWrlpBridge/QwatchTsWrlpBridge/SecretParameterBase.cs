using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwatchTsWrlpBridge
{
    public class SecretParameterBase
    {
        public virtual string Location => "location0001";           // location name (lower character start, abc0123 only. cannot use upper case and symboles)
        public virtual string UserName => "DigestAuthUserNameHere"; // Your WebCamera's User Name
        public virtual string Password => "DigestAuthPasswordHere"; // Your WebCamera's Password
        public virtual string Url => "http://192.168.1.1:47826";    // Web Camera's HTTP Server
        public virtual string AzureStorageConnectionString => "DefaultEndpointsProtocol=https;AccountName=hogehogehoge;AccountKey=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa==;EndpointSuffix=core.windows.net";
    }

    /// <summary>
    /// Private secret key (Please override key texts for your secret)
    /// </summary>
    public partial class MySecretParameter : SecretParameterBase
    {
    }
}
