using RabbitMQ.Client;

namespace FileCreateWorkerService.Services
{
    public class RabbitMQClientService : IDisposable
    {
      //Bu sınıf sadece RabbitMQ ile bağlantı kursun ve geriye bir tane channel döndürsün
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string QueueName = "queue-excel-file";
        private readonly ILogger<RabbitMQClientService> _logger;
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();
            //is keyword ile channel içerisindeki özelliğine erişebiliriz
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }
            _channel = _connection.CreateModel();

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
