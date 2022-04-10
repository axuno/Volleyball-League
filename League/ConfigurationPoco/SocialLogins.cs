namespace League.ConfigurationPoco
{
    public class SocialLogins
    {
        public Microsoft Microsoft { get; set; } = new();
        public Google Google { get; set; } = new();
        public Facebook Facebook { get; set; } = new();
    }

    public class Facebook
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }

    public class Google
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class Microsoft
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
