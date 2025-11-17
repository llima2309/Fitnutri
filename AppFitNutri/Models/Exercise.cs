namespace AppFitNutri.Models;

public class Exercise
{
    public string Name { get; set; }
    public string Sets { get; set; }
    public string Reps { get; set; }
    public string Details { get; set; }
    public string VideoUrl { get; set; } = "https://fitnutri-videos.s3.us-east-1.amazonaws.com/video.mp4";
}

