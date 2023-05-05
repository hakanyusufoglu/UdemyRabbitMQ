using RabbitMQ.Client;
using System.Text;

namespace UdemyRabbitMQ.TopicExchangePublisher
{
	internal class Program
	{
		public enum LogNames { Cricital = 1, Error = 2, Warning = 3, Info = 4 }
		static void Main(string[] args)
		{
			var factory = new ConnectionFactory();
			factory.Uri = new Uri("amqps://chgkpvyr:KkEiat6wFkGRT67kXSf2mbSlcfuuopXI@cow.rmq2.cloudamqp.com/chgkpvyr");

			using var connection = factory.CreateConnection();

			//Bir bağlantıya kanal channel oluşturuyoruz.
			var channel = connection.CreateModel();

			//Exchange oluşturuyoruz...
			channel.ExchangeDeclare("logs-topic", durable: true, type: ExchangeType.Topic);

			Random rnd = new Random();

			//her döngüde burdaki değeri rabbite gönderiyoruz.
			Enumerable.Range(1, 50).ToList().ForEach(x =>
			{
				LogNames log1 = (LogNames)rnd.Next(1, 5);
				LogNames log2 = (LogNames)rnd.Next(1, 5);
				LogNames log3 = (LogNames)rnd.Next(1, 5);

				var routeKey = $"{log1}.{log2}.{log3}";
				//mesajımızı oluşturuyoruz
				string message = $"Log-type: {log1}-{log2}-{log3}"; 
				var messageBody = Encoding.UTF8.GetBytes(message);
				//bu mesajı channel üzerindengöndericez exhange kullanmadığım için empty kullanıyorum. Empty olmasaydı Default exchange olacaktı.
				channel.BasicPublish("logs-topic", routeKey, null, messageBody);

				Console.WriteLine($"log Gönderilmiştir : {message}");

			});
		}
	}
}