namespace TagFinder
{
    public enum LoginResult
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
