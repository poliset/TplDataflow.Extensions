using System;
using Microsoft.Practices.TransientFaultHandling;

namespace TplDataflow.Tests
{
    public class HandleAnyException : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            return true;
        }
    }
}
