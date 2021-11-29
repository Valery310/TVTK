namespace TVTK.Enums
{
    public enum ExtensionsImageMediaFile
    {
        png,
        jpg,
        jpeg
    }

    public enum ExtensionsAudioMediaFile
    {
        mp3,
        wav
    }

    public enum ExtensionsVideoMediaFile
    {
        mp4,
        avi,
        mkv,
        mov
    }

    public enum TypeMediaFile
    {
        Audio,
        Video,
        Image,
        Unknown
    }

    public enum TypePlaying
    {
        Random,
        Sequence,
        Loop
    }

    public enum TypeWork
    {
        Local,
        Network,
        Mixed
    }

    public enum TypeContent
    {
        Adv,
        News,
        Photo,
        Statistics,
        Default
    }

    public enum DayOfWeek 
    { 
        Monday, 
        Tuesday, 
        Wednesday, 
        Thursday, 
        Friday, 
        Saturday, 
        Sunday, 
        All,
        Weekdays,
        Weekends

    }
}
