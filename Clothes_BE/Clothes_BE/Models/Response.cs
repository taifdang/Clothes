namespace Clothes_BE.Models
{
    public class Response
    {
        //status code
        public int status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}
