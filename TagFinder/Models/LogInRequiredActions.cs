namespace TagFinder
{
    public enum LogInRequiredActions
    {
        Success,
        PhoneNumberRequired,
        SMSVerifyRequired,
        EmailVerifyRequired,
        TwoFactorRequired,
        VerificationFailed,
        FullLogInReqired,
        Error
    }
}
