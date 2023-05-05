using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Subscriber
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


			//kaç kaç veri göndereceğimiz belirilyoruz.
			//ilk parametre: bana herhangi bir boyuttaki mesajı gönderebilirsin,
			//ikicni: her bir subcriber'e birer mesaj gelsin
			//üçüncü: kaç subcriber varsa tek bir seferde 5 olacak şekilde ayarlar (true ise); false ise kaç tane varsa o kadar yani 2 tane subcriber varsa 5,5 gönderecektir.
			channel.BasicQos(0, 1, false);


			var consumer = new EventingBasicConsumer(channel);
			var queueName = channel.QueueDeclare().QueueName;
			Dictionary<string, object> headers = new Dictionary<string, object>();
			headers.Add("format", "pdf");
			headers.Add("shape", "a4");
			headers.Add("x-match", "any"); //all dersek publisher tarafında tüm key, value değerlerinin uyuşması gerekiyor diyoruz. eğer any dersek tek bir tanesinin uyuşması yeterli oluyor.

			channel.QueueBind(queueName, "header-exchange", string.Empty, headers);
			//autoAck = Subscriber doğruda yanlışsa işlense bilgi silinir. false yaparsak sen bu bilgiyi kuyruktan silme diyoruz. Böylece emin oluyoruz.
			channel.BasicConsume(queueName, false, consumer);

			Console.WriteLine("Loglar dinleniyor...");
			consumer.Received += (object sender, BasicDeliverEventArgs e) =>
			{
				var message = Encoding.UTF8.GetString(e.Body.ToArray());
				Product? product = JsonSerializer.Deserialize<Product>(message);
				Thread.Sleep(1500);
				Console.WriteLine($"Gelen Mesaj: {product?.Id}-{ product?.Name} -{ product?.Price}-{ product?.Stock}");

				channel.BasicAck(e.DeliveryTag, false);
			};

			Console.ReadLine();
		}
	}
}