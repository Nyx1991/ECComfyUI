using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECComfyUI.Models
{
    internal class ECCUIException : Exception
    {
        public ECCUIException(string message, ECCUIErrorMessage errorMessage = null) : base(message)
        {
            ECCUIErrorMessage = errorMessage;
        }
        public ECCUIErrorMessage ECCUIErrorMessage { get; private set; }
    }
}
