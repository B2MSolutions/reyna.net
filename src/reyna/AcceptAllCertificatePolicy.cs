﻿namespace Reyna
{
    using System.Net;
    using System.Security.Cryptography.X509Certificates;

    public class AcceptAllCertificatePolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(
            ServicePoint srvPoint,
            X509Certificate certificate,
            WebRequest request,
            int certificateProblem)
        {
            return true;
        }
    }
}
