namespace Reyna
{
    using System;
    using Reyna.Interfaces;

    public class RegistryContactInformation : IContactInformation
    {
        private readonly ITimeProvider timeProvider;

        public RegistryContactInformation(IRegistry registry, string key)
        {
            this.Registry = registry;
            this.ContactInformationKeyName = key;
            this.timeProvider = new TimeProvider();
        }

        public System.DateTime LastContactAttempt
        {
            get
            {
                long lastContactAttemptEpoc = this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastContactAttempt", 0);
                return this.timeProvider.GetDateTimeFromEpoc(lastContactAttemptEpoc, DateTimeKind.Utc);
            }

            set
            {
                long epocInMilliseconds = this.timeProvider.GetEpochInMilliSeconds(value, DateTimeKind.Local);
                
                this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastContactAttempt", epocInMilliseconds);
            }
        }

        public System.DateTime LastSuccessfulContact
        {
            get
            {
                long lastastSuccessfulContactEpoc = this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastSuccessfulContact", 0);
                return this.timeProvider.GetDateTimeFromEpoc(lastastSuccessfulContactEpoc, DateTimeKind.Utc);
            }
            
            set
            {
                long epocInMilliseconds = this.timeProvider.GetEpochInMilliSeconds(value, DateTimeKind.Local);

                this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastSuccessfulContact", epocInMilliseconds);
            }
        }

        public Reyna.Interfaces.Result LastContactResult
        {
            get
            {
                var result = this.Registry.GetString(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastContactResult", string.Empty);
                try
                {
                    return (Result)Enum.Parse(typeof(Result), result, false);
                }
                catch (Exception)
                {
                    return Reyna.Interfaces.Result.NotConnected;
                }
            }
            
            set
            {
                this.Registry.SetString(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastContactResult", value.ToString());
            }
        }
    
        private IRegistry Registry { get; set; }

        private string ContactInformationKeyName { get; set; }
    }
}
