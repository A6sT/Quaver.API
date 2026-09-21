namespace Quaver.API.Maps.AutoMod.Issues.Audio
{
    public class AutoModIssueAudioBitrate : AutoModIssue
    {
        public override AutoModIssueCategory Category { get; protected set; } = AutoModIssueCategory.Files;

        public string AudioFormat { get; }

        public int MaxBitrate { get; }

        public AutoModIssueAudioBitrate()
            : this("MP3", global::Quaver.API.Maps.AutoMod.AutoMod.MaxMp3AudioBitrate)
        {
        }

        public AutoModIssueAudioBitrate(string audioFormat, int maxBitrate) : base(AutoModIssueLevel.Ranking)
        {
            AudioFormat = audioFormat;
            MaxBitrate = maxBitrate;
            Text = $"The {audioFormat} audio bitrate must be {maxBitrate}kbps or lower.";
        }
    }
}
