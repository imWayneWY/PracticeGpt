using System;
namespace PracticeGpt.Data;

public enum ChatAuthorType
{
    Gpt,
    User,
    System,
}
public class Chat
{
    public DateTime Time { get; set; }

    public string? ChatContent { get; set; }

    public ChatAuthorType authorType { get; set; }
}


