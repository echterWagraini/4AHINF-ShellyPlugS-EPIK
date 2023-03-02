namespace BlazorApp1
{
    public class Plug
    { 
        public bool ison { get; set; }
        public bool has_timer { get; set; }
        public float timer_started { get; set; }
        public float timer_duration { get; set; }
        public float timer_remaining { get; set; }
        public bool overpower { get; set; }
        public string source { get; set; }
    }
}
