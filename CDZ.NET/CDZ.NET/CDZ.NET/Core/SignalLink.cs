using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDZNET.Core
{
    public class SignalLink
    {
        public Signal from;
        public Signal to;
        public SignalLink(Signal from, Signal to)
        {
            this.from = from;
            this.to = to;
        }

        /// <summary>
        /// Copy the prediction of "from" to the real of "to"
        /// </summary>
        public void FeedForward()
        {
            Array.Copy(from.prediction, to.reality, from.prediction.Length);
        }        
        
        /// <summary>
        /// Copy the prediction of "to" to the real of "from"
        /// </summary>
        public void FeedBack()
        {
            Array.Copy(to.prediction, from.reality, to.prediction.Length);
        }
    }
}
