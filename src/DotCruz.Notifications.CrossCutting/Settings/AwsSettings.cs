using System.ComponentModel.DataAnnotations;

namespace DotCruz.Notifications.CrossCutting.Settings;

public class AwsSettings
{
    [Required]
    public string Region { get; set; } = string.Empty;
    [Required]
    public string AccessKey { get; set; } = string.Empty;
    [Required]
    public string SecretKey { get; set; } = string.Empty;
    [Required]
    public string SqsQueueUrl { get; set; } = string.Empty;
    [Required]
    public string SqsQueueArn { get; set; } = string.Empty;
    [Required]
    public string SchedulerRoleArn { get; set; } = string.Empty;
    [Required]
    public string SmtpParameterPath { get; set; } = string.Empty;
}
