namespace ElemezTest
{
    using ElemezServiceInterface;
    using ElemezServices;
    using ElemezSupport;
    using Moq;
    using Xunit;

    public class GivenARegistry
    {
        public GivenARegistry()
        {
            this.Logger = new Mock<ILogger>();

            this.Registry = new Registry(this.Logger.Object);
            
            this.Logger.Setup(l => l.Verbose(It.IsAny<string>()));

            GivenARegistry.TearDown();
        }

        private Mock<ILogger> Logger { get; set; }

        private Registry Registry { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.Registry);
        }

        [Fact]
        public void WhenCallingGetStringShouldLogVerbose()
        {
            this.Registry.GetString(Microsoft.Win32.Registry.LocalMachine, "KEY", "VALUENAME", null);

            this.Logger.Verify(l => l.Verbose("Registry.GetString hive {0}, key {1}, valueName{2}", Microsoft.Win32.Registry.LocalMachine, "KEY", "VALUENAME"), Times.Once());
        }

        [Fact]
        public void WhenCallingSetStringShouldLogVerbose()
        {
            this.Registry.SetString(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAME", "VALUE");

            this.Logger.Verify(l => l.Verbose("Registry.SetString hive {0}, key {1}, valueName{2}, value{3}", Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAME", "VALUE"), Times.Once());
        }

        [Fact]
        public void WhenCallingGetQWordShouldLogVerbose()
        {
            this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, "KEY", "VALUENAME", 0);

            this.Logger.Verify(l => l.Verbose("Registry.GetQWord hive {0}, key {1}, valueName{2}", Microsoft.Win32.Registry.LocalMachine, "KEY", "VALUENAME"), Times.Once());
        }

        [Fact]
        public void WhenCallingSetQWordShouldLogVerbose()
        {
            this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAME", long.MaxValue);

            this.Logger.Verify(l => l.Verbose("Registry.SetQWord hive {0}, key {1}, valueName{2}, value{3}", Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAME", long.MaxValue), Times.Once());
        }

        [Fact]
        public void WhenCallingGetDWordShouldLogVerbose()
        {
            this.Registry.GetDWord(Microsoft.Win32.Registry.LocalMachine, "KEY", "VALUENAME", 0);

            this.Logger.Verify(l => l.Verbose("Registry.GetDWord hive {0}, key {1}, valueName{2}", Microsoft.Win32.Registry.LocalMachine, "KEY", "VALUENAME"), Times.Once());
        }

        [Fact]
        public void WhenCallingSetDWordShouldLogVerbose()
        {
            this.Registry.SetDWord(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAME", 42);

            this.Logger.Verify(l => l.Verbose("Registry.SetDWord hive {0}, key {1}, valueName{2}, value{3}", Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAME", 42), Times.Once());
        }

        [Fact]
        public void WhenCallingGetStringShouldReturnExpected()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                key.SetValue("VALUENAME", "VALUE");
            }

            var actual = this.Registry.GetString(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAME", null);

            Assert.Equal("VALUE", actual);
        }

        [Fact]
        public void WhenCallingGetStringAndValueNotExistsShouldReturnDefault()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                key.SetValue("VALUENAME", "VALUE");
            }

            var actual = this.Registry.GetString(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAMENOTEXISTS", "DefaultValue");

            Assert.Equal("DefaultValue", actual);
        }

        [Fact]
        public void WhenCallingGetStringAndKeyNotExistShouldReturnNull()
        {
            var actual = this.Registry.GetString(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist", "VALUENAME", null);

            Assert.Null(actual);
        }

        [Fact]
        public void WhenCallingSetStringShouldWriteToRegistry()
        {
            this.Registry.SetString(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValue", "test");

            string value = null;

            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                value = key.GetValue("TestValue") as string;
            }

            Assert.Equal("test", value);
        }

        [Fact]
        public void WhenCallingSetQWordShouldWriteToRegistry()
        {
            this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueNumber", long.MaxValue);

            long? value = 0;

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Key"))
            {
                value = key.GetValue("TestValueNumber") as long?;
            }

            Assert.Equal(long.MaxValue, value.Value);
        }

        [Fact]
        public void WhenCallingGetQWordShouldReturnExpected()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                key.SetValue("TestValueNumber", long.MaxValue, Microsoft.Win32.RegistryValueKind.QWord);
            }

            long actual = this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueNumber", 0);

            Assert.Equal(long.MaxValue, actual);
        }

        [Fact]
        public void WhenCallingSetDWordShouldWriteToRegistry()
        {
            this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueDWord", 100);

            long? value = 0;

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Key"))
            {
                value = key.GetValue("TestValueDWord") as long?;
            }

            Assert.Equal(100, value.Value);
        }

        [Fact]
        public void WhenCallingSetBinaryShouldWriteToRegistry()
        {
            this.Registry.SetBinary(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueBinary", new byte[] { 100, 200 });

            byte[] value = new byte[] { };

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Key"))
            {
                value = key.GetValue("TestValueBinary") as byte[];
            }

            Assert.Equal(new byte[] { 100, 200 }, value);
        }

        [Fact]
        public void WhenCallingGetDWordShouldReturnExpected()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                key.SetValue("TestValueDWord", 42, Microsoft.Win32.RegistryValueKind.DWord);
            }

            long actual = this.Registry.GetDWord(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueDWord", 0);

            Assert.Equal(42, actual);
        }

        [Fact]
        public void WhenCallingGetBinaryShouldReturnExpected()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                key.SetValue("TestValueBinary", new byte[] { 100, 200 }, Microsoft.Win32.RegistryValueKind.Binary);
            }

            var actual = this.Registry.GetBinary(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueBinary", null);

            Assert.Equal<byte[]>(new byte[] { 100, 200 }, actual);
        }

        [Fact]
        public void WhenCallingGetQWordAndKeyNotExistShouldReturnZero()
        {
            var actual = this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist", "VALUENAME", 0);

            Assert.Equal(0, actual);
        }

        [Fact]
        public void WhenCallingGetDWordAndKeyNotExistShouldReturnZero()
        {
            var actual = this.Registry.GetDWord(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist", "VALUENAME", 0);

            Assert.Equal(0, actual);
        }

        [Fact]
        public void WhenCallingGetBinaryAndKeyNotExistShouldReturnEmptyByteArray()
        {
            var emptyArray = new byte[] { };
            var actual = this.Registry.GetBinary(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist", "VALUENAME", emptyArray);

            Assert.Equal(0, actual.Length);
        }

        [Fact]
        public void WhenCallingGetBinaryAndKeyNotExistShouldReturnDefaultValue()
        {
            var actual = this.Registry.GetBinary(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist", "VALUENAME", null);

            Assert.Null(actual);
        }

        [Fact]
        public void WhenCallingDeleteValueAndKeyNotExistShouldNotThrow()
        {
            this.Registry.DeleteValue(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist", "Value");
        }

        [Fact]
        public void WhenCallingDeleteValueAndValueNotExistShouldNotThrow()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
            }

            this.Registry.DeleteValue(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "VALUENAMENotExist");
        }

        [Fact]
        public void WhenCallingDeleteValueAndValueExistShouldDelete()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                key.SetValue("TestValueDWord", 42, Microsoft.Win32.RegistryValueKind.DWord);
            }

            this.Registry.DeleteValue(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueDWord");

            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                var value = key.GetValue("TestValueDWord", -1);
                Assert.Equal(-1, value);
            }
        }

        [Fact]
        public void WhenCallingKeyExistsShouldReturnTrueIfKeyExistsInRegistry()
        {
            var keyExists = false;
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                keyExists = this.Registry.KeyExists(Microsoft.Win32.Registry.LocalMachine, @"Software\Key");
            }

            Assert.True(keyExists);
        }

        [Fact]
        public void WhenCallingKeyExistsAndKeyNotExistsInRegistryShouldReturnFalse()
        {
             var keyExists = this.Registry.KeyExists(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist");

            Assert.False(keyExists);
        }

        [Fact]
        public void WhenCallingGetStringsAndKeyNotExistShouldReturnEmptyStringArray()
        {
            var emptyArray = new string[] { };
            var actual = this.Registry.GetStrings(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist", "VALUENAME", emptyArray);

            Assert.Equal(0, actual.Length);
        }

        [Fact]
        public void WhenCallingGetStringsAndKeyNotExistShouldReturnDefaultValue()
        {
            var actual = this.Registry.GetStrings(Microsoft.Win32.Registry.LocalMachine, @"Software\KeyNotExist", "VALUENAME", null);

            Assert.Null(actual);
        }

        [Fact]
        public void WhenCallingGetStringsShouldReturnExpected()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                key.SetValue("TestValueMSz", new string[] { "A", "B C" }, Microsoft.Win32.RegistryValueKind.MultiString);
            }

            var actual = this.Registry.GetStrings(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueMSz", null);

            Assert.Equal<string[]>(new string[] { "A", "B C" }, actual);
        }

        [Fact]
        public void WhenCallingSetStringsShouldWriteToRegistry()
        {
            this.Registry.SetStrings(Microsoft.Win32.Registry.LocalMachine, @"Software\Key", "TestValueMSz", new string[] { "D E", "F G" });

            var value = new string[] { };

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Key"))
            {
                value = key.GetValue("TestValueMSz") as string[];
            }

            Assert.Equal(new string[] { "D E", "F G" }, value);
        }

        [Fact]
        public void WhenCallingDeleteKeyTreeShouldDeleteKeyTree()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
                key.CreateSubKey("key1");
            }

            this.Registry.DeleteKeyTree(Microsoft.Win32.Registry.LocalMachine, @"Software\Key");

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Key"))
            {
                Assert.Null(key);
            }
        }

        [Fact]
        public void WhenCallingDeleteKeyTreeForOneKeyRootShouldDeleteKeyTree()
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Key"))
            {
            }

            this.Registry.DeleteKeyTree(Microsoft.Win32.Registry.LocalMachine, @"Software\Key");

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Key"))
            {
                Assert.Null(key);
            }
        }

        private static void TearDown()
        {
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Key", false);
        }
    }
}
