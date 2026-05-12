namespace EtaskMinstry.Api.Dtos.Common
{
    public class ErrorItem
    {
        public string Field { get; set; }
        public string Message { get; set; }

        public ErrorItem() { }

        public ErrorItem(string field, string message)
        {
            Field = field;
            Message = message;
        }
    }
}
