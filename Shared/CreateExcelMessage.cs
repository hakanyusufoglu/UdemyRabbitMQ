namespace Shared
{
    public class CreateExcelMessage
    {
        //Hangi kullanıcı bu mesajı gönderdi.
        public string UserId { get; set; }
        public int FileId { get; set; }

    }
}
