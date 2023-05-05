using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

namespace Publisher
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

			//Exchange oluşturuyoruz...
			channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

			Dictionary<string, object> headers = new Dictionary<string, object>();
			headers.Add("format", "pdf");
			headers.Add("shape2", "a4");

			var properties = channel.CreateBasicProperties();
			properties.Headers = headers;
			properties.Persistent = true;//mesajlar kalıcı hale getirmektedir.

			var product = new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 10 };
			var productJsonString=JsonSerializer.Serialize(product);
			channel.BasicPublish("header-exchange", string.Empty, properties, Encoding.UTF8.GetBytes(productJsonString));
			Console.WriteLine("Mesaj gönderilmiştir");
			Console.ReadLine();

		}
	}
}