using Microsoft.AspNetCore.DataProtection;

namespace Oqtane.Security
{
    public sealed class DataProtector
    {
        private static readonly string _purpose = "Oqtane.Security";

        private readonly IDataProtector _dataProtector;

        public DataProtector(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(_purpose);
        }

        public string Protect(string plaintext) => _dataProtector.Protect(plaintext);

        public string Unprotect(string protectedData) => _dataProtector.Unprotect(protectedData);
    }
}
