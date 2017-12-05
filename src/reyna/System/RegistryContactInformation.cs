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

        public System.DateTime? LastContactAttempt
        {
            get
            {
                long lastContactAttemptEpoc = this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastContactAttempt", 0);
                if (lastContactAttemptEpoc == 0)
                {
                    return null;
                }

                return this.timeProvider.GetDateTimeFromEpoc(lastContactAttemptEpoc, DateTimeKind.Utc);
            }

            set
            {
                if (value == null)
                {
                    this.Registry.DeleteValue(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastContactAttempt");
                    return;
                }

                long epocInMilliseconds = this.timeProvider.GetEpochInMilliSeconds((DateTime)value, DateTimeKind.Local);
                
                this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastContactAttempt", epocInMilliseconds);
            }
        }

        public System.DateTime? LastSuccessfulContact
        {
            get
            {
                long lastSuccessfulContactEpoc = this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastSuccessfulContact", 0);

                if (lastSuccessfulContactEpoc == 0)
                {
                    return null;
                }

                return this.timeProvider.GetDateTimeFromEpoc(lastSuccessfulContactEpoc, DateTimeKind.Utc);
            }
            
            set
            {
                if (value == null)
                {
                    this.Registry.DeleteValue(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastSuccessfulContact");
                    return;
                }

                long epocInMilliseconds = this.timeProvider.GetEpochInMilliSeconds((DateTime)value, DateTimeKind.Local);

                this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, this.ContactInformationKeyName, "LastSuccessfulContact", epocInMilliseconds);
            }
        }

        public Reyna.Interfaces.Result? LastContactResult
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
                    return null;
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
