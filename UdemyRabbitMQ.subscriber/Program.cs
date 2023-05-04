using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace UdemyRabbitMQ.subscriber
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

			//subscriber çıktığında kuyruk sonlandırılsın
			var randomQueueName = channel.QueueDeclare().QueueName;

			channel.QueueBind(randomQueueName, "logs-fanout", "", null);

			//kaç kaç veri göndereceğimiz belirilyoruz.
			//ilk parametre: bana herhangi bir boyuttaki mesajı gönderebilirsin,
			//ikicni: her bir subcriber'e birer mesaj gelsin
			//üçüncü: kaç subcriber varsa tek bir seferde 5 olacak şekilde ayarlar (true ise); false ise kaç tane varsa o kadar yani 2 tane subcriber varsa 5,5 gönderecektir.
			channel.BasicQos(0, 1, false);


			//mesajları iletebilmek için queue oluşturuyoruz.
			//channel.QueueDeclare("kuyruk_ismi",
			//veri_kalıcı_olsunmu,
			//bu_kuyruga_Sadece_bu_kanal_mı_baglansin,
			//subscriber down olduğunda kuyruk silinsin mi?)

			//channel.QueueDeclare("hello-queue", true, false, false);

			//yukarıdaki kodu silersek hata alabiliriz. ancak publisherin bu kuyruğu kesin oluşturduğundan eminsek yukarıdakini silebiliriz. bunu publisher haber vericek

			var consumer = new EventingBasicConsumer(channel);

			//autoAck = Subscriber doğruda yanlışsa işlense bilgi silinir. false yaparsak sen bu bilgiyi kuyruktan silme diyoruz. Böylece emin oluyoruz.
			channel.BasicConsume(randomQueueName, true, consumer);

			Console.WriteLine("Loglar dinleniyor...");
			consumer.Received += (object sender, BasicDeliverEventArgs e) =>
			{
				var message = Encoding.UTF8.GetString(e.Body.ToArray());

				Thread.Sleep(1500);
				Console.WriteLine("Gelen Mesaj:" + message);

				//artık kuyruktan silinsin diyoruz. tek bir mesajı işlediğimizi için false yapıyoruz;
				channel.BasicAck(e.DeliveryTag, false);
			};

			Console.ReadLine();
		}

		private static void Consumer_Received(object? sender, BasicDeliverEventArgs e)
		{
			var message = Encoding.UTF8.GetString(e.Body.ToArray());
			Console.WriteLine("Gelen Mesaj:" + message);
			
		}
	}
}