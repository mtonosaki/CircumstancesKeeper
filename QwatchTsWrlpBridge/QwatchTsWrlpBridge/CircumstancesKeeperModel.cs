using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwatchTsWrlpBridge
{
    public class CircumstancesKeeperModel
    {
		public string Key { get; set; }
		public DateTime EventTime { get; set; }
		public byte[] ImageData { get; set; }
		public string ImageType { get; set; }
		public string ErrorMessage { get; set; }
	}
}
