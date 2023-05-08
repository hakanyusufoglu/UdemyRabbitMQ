using System.ComponentModel.DataAnnotations.Schema;

namespace UdemyRabbitMQWeb.ExcelCreate.Models
{
    //Excel dosyası oluşturuluyor veya oluşturuldu gibi file durumlarını tutmak için bu enum oluşturuldu.
    public enum FileStatus
    {
        Creating,
        Completed
    }
    public class UserFile
    {
        public int Id { get; set; } 
        public string UserId{ get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public FileStatus FileStatus { get; set; }

        //Veritabanında tabloya bu özelliği ekleme diyoruz.
        [NotMapped]
        public string GetCreatedDate => CreatedDate.HasValue ? CreatedDate.Value.ToShortDateString() : "-"; //bir tarihe sahipse tarihi döndür değilse geriye çizgi döndür
    }
}
