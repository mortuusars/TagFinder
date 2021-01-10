namespace TagFinder.Updater
{
    public enum UpdateStates
    {
        DeletingOldFiles,
        DownloadingFiles,
        UnzippingFiles,
        DeletingTempFiles,
        Finished,
        DownloadingFailed
    }
}
