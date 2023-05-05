using RabbitMQ.Client;

namespace UdemyRabbitMQWeb.Watermark.Services
{
	public class RabbitMQClientService:IDisposable
	{
		//RabbitMQ bağlantı işlemleri ve exchange ayarlamaları için sınıfın property'lerini belirliyoruz.
		//Burası exchange'in oluşturulması, kuyruğun bind edilmesi vs hep bu sınıf üzerinden gerçekleştirilecektir

		//ConnectionFactory DI (Dependency Injection ile alınsın);
		private readonly ConnectionFactory _connectionFactory;
		private IConnection _connection;
		private IModel _channel;
		public static string ExchangeName = "ImageDirectExchange";
		public static string RoutingWatermark="watermark-route-image";
		public static string QueueName = "queue-watermark-image";
		private readonly ILogger<RabbitMQClientService> _logger;
		public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
		{
			_connectionFactory = connectionFactory;
			_logger = logger;
			Connect();
		}

		public IModel Connect()
		{
			_connection = _connectionFactory.CreateConnection();
			//is keyword ile channel içerisindeki özelliğine erişebiliriz
			if (_channel is { IsOpen:true })
			{
				return _channel;
			}
			_channel = _connection.CreateModel();
			//exchange oluşturuluyor.
			_channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);
			//kuruk oluşturuluyor.
			_channel.QueueDeclare(QueueName, true, false, false);
			_channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWatermark);
			_logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");
			return _channel;
		}
		//Dispose olduğunda rabbitmq ile olanları kapat
		public void Dispose()
		{
			_channel?.Close();
			_channel?.Dispose();
			//_channel = default; //yani null'a set ediliyor default sol taraftaki değerin null'ını döndürür
			_connection?.Close();
			_connection?.Dispose();
			//_connection = default;
			_logger.LogInformation("RabbitMQ ile bağlantı koptu...");

		}
	}
}
