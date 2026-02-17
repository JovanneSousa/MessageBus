namespace Bus
{
    public class RabbitSettings
    {
        public string Url { get; set; }
        public Dictionary<string, string> Exchange { get; set; } = new();
        public Dictionary<string, string> RoutingKey { get; set; } = new();
        public Dictionary<string, string> Queue { get; set; } = new();
    }
}
