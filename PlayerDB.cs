namespace QCS
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class NameHistory
    {
        public string? name { get; set; }
        public long? changedToAt { get; set; }
    }

    public class Meta
    {
        public List<NameHistory>? name_history { get; set; }
    }

    public class Player
    {
        public Meta? meta { get; set; }
        public string? username { get; set; }
        public string? id { get; set; }
        public string? raw_id { get; set; }
        public string? avatar { get; set; }
    }

    public class Data
    {
        public Player? player { get; set; }
    }

    public class PlayerDB
    {
        public string? code { get; set; }
        public string? message { get; set; }
        public Data? data { get; set; }
        public bool? success { get; set; }
    }
}