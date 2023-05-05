using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace UdemyRabbitMQ.TopicExchangeSubscriber
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
			//var routeKey = "*.*.Warning"; //sonu Warning olan başı ve ortası önemli olmayan tüm veriler kuyruğuma gelsin

			var routeKey = "*.Error.*"; //Ortasında error olan başı ve sonu önemli olmayan tüm veriler kuyruğuma gelsin
			channel.QueueBind(queueName, "logs-topic", routeKey);
			//autoAck = Subscriber doğruda yanlışsa işlense bilgi silinir. false yaparsak sen bu bilgiyi kuyruktan silme diyoruz. Böylece emin oluyoruz.
			channel.BasicConsume(queueName, false, consumer);

			Console.WriteLine("Loglar dinleniyor...");
			consumer.Received += (object sender, BasicDeliverEventArgs e) =>
			{
				var message = Encoding.UTF8.GetString(e.Body.ToArray());

				Thread.Sleep(1500);
				Console.WriteLine("Gelen Mesaj:" + message);

				File.AppendAllText("logs-critical.txt", message + "\n");
				//artık kuyruktan silinsin diyoruz. tek bir mesajı işlediğimizi için false yapıyoruz;
				channel.BasicAck(e.DeliveryTag, false);
			};

			Console.ReadLine();
		}
	}
}