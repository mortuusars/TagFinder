namespace TagFinder.Core.InstagramAPI
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
