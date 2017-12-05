namespace Reyna
{
    using System;
    using Reyna.Interfaces;

    public interface IContactInformation
    {
        DateTime? LastContactAttempt { get; set; }

        DateTime? LastSuccessfulContact { get; set; }
        
        Result? LastContactResult { get; set; }
    }
}
