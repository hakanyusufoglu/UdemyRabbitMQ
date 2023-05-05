using RabbitMQ.Client;
using System.Text;

namespace UdemyRabbitMQ.publisher
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var factory = new ConnectionFactory();
			factory.Uri = new Uri("amqps://chgkpvyr:KkEiat6wFkGRT67kXSf2mbSlcfuuopXI@cow.rmq2.cloudamqp.com/chgkpvyr");

			using var connection = factory.CreateConnection();

			//Bir bağlantıya kanal channel oluşturuyoruz.
			var channel = connection.CreateModel();

			//mesajları iletebilmek için queue oluşturuyoruz.
			//channel.QueueDeclare("kuyruk_ismi",
			//veri_kalıcı_olsunmu,
			//bu_kuyruga_Sadece_bu_kanal_mı_baglansin,
			//subscriber down olduğunda kuyruk silinsin mi?)
			//channel.QueueDeclare("hello-queue", true, false, false);


			//Exchange oluşturuyoruz...
			channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Direct);


			//her döngüde burdaki değeri rabbite gönderiyoruz.
			Enumerable.Range(1, 50).ToList().ForEach(x => {
				//mesajımızı oluşturuyoruz
				string message = $"Log Message {x}"; //mesajlar rabbite byte olarak gönderilir. Böylece pdf bile gönderilebilir.

				//mesajı kuyruğa dönüştürdük.
				var messageBody = Encoding.UTF8.GetBytes(message);

				//bu mesajı channel üzerindengöndericez exhange kullanmadığım için empty kullanıyorum. Empty olmasaydı Default exchange olacaktı.
				channel.BasicPublish("logs-fanout",string.Empty, null, messageBody);

				Console.WriteLine($"Mesaj Gönderilmiştir : {message}");

			});



			Console.ReadLine();
		}
	}
}