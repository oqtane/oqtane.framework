namespace Oqtane.Shared
{
    public enum ExternalLoginStatus
    {
        Success,
        InvalidEmail,
        DuplicateEmail,
        UserNotCreated,
        UserDoesNotExist,
        ProviderKeyMismatch,
        VerificationRequired
    }
}
