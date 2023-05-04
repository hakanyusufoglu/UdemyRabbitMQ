using RabbitMQ.Client;
using System.Text;

namespace UdemyRabbitMQ.DirectExchangePublisher
{
	internal class Program
	{
		public enum LogNames { Cricital=1,Error=2,Warning=3,Info=4}
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
			channel.ExchangeDeclare("logs-direct", durable: true, type: ExchangeType.Fanout);

			Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
			{
				var routeKey = $"route-{x}";

				var queueName = $"direct-queue-{x}"; //kuyruğun ismini oluşturduk
				channel.QueueDeclare(queueName, true, false, false);//kuyruğu tanımladık
				channel.QueueBind(queueName, "logs-direct", routeKey,null);//kuyruğu bind ettik
			});


			//her döngüde burdaki değeri rabbite gönderiyoruz.
			Enumerable.Range(1, 50).ToList().ForEach(x => {
				LogNames log = (LogNames)new Random().Next(1, 4);
				//mesajımızı oluşturuyoruz
				string message = $"Log-type: {log}"; //mesajlar rabbite byte olarak gönderilir. Böylece pdf bile gönderilebilir.

				//mesajı kuyruğa dönüştürdük.
				var messageBody = Encoding.UTF8.GetBytes(message);

				//ilgili mesajın route'ı belirleniyor.

				var routeKey = $"route-{log}";


				//bu mesajı channel üzerindengöndericez exhange kullanmadığım için empty kullanıyorum. Empty olmasaydı Default exchange olacaktı.
				channel.BasicPublish("logs-direct",routeKey, null, messageBody);

				Console.WriteLine($"log Gönderilmiştir : {message}");

			});



			Console.ReadLine();
		}
	}
}