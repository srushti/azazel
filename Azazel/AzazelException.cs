using System;

namespace Azazel {
    public class AzazelException : Exception {
        public AzazelException(string message) : base(message) {}
        public AzazelException() {}
        public AzazelException(string message, Exception innerException) : base(message, innerException) {}
    }
}