using ClosedXML.Excel;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Data;
using System.Text;
using System.Text.Json;
using Product = FileCreateWorkerService.Models.Product;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private RabbitMQClientService _rabbitmqClientService;
        //Workerda direkt olarak AdventureDbContext kullanamadýðýmýzdan yani scope olarak kullanamadýðýmýzdan serviceProvider üzerinden context'e eriþeceðiz
        private readonly IServiceProvider _serviceProvider;
        private IModel _channel;
        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitmqClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitmqClientService = rabbitmqClientService;
            _serviceProvider = serviceProvider;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //Baðlantý kurulacak ve 1,1 gönderilecek
            _channel=_rabbitmqClientService.Connect();
            _channel.BasicQos(0, 1, false);
            return base.StartAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName,false,consumer);
            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            //Ýþlemin uzun sürdüðünü göstermek için eklendi
            await Task.Delay(5000);
            //kuyruktan mesaj alýnýyor.
            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            using var ms = new MemoryStream();
            var wb = new XLWorkbook();
            var ds = new DataSet();
            ds.Tables.Add(GetTable("products"));
            wb.Worksheets.Add(ds);
            wb.SaveAs(ms);

            MultipartFormDataContent multipartFormDataContent = new();
            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()),"file",Guid.NewGuid().ToString()+".xlsx");
            var baseUrl = "https://localhost:7104/api/files";
            using(var httpclient = new HttpClient())
            {
                var response = await httpclient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}",multipartFormDataContent);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File (Id: {createExcelMessage.FileId}) was created by successfull");
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            }
        }

        //Excel Tablo oluþturma
        private DataTable GetTable(string tableName) 
        {
            List<Product> products;
            //veri tabanýna baðlanýlma iþlemi
            using (var scope=_serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();
                products = context.Products.ToList();
            }
            DataTable table = new DataTable { TableName= tableName };
            //tablo sütunlarý
            table.Columns.Add("ProductId",typeof(int));
            table.Columns.Add("Name",typeof(string));
            table.Columns.Add("ProductNumber",typeof(string));
            table.Columns.Add("Color",typeof(string));
            products.ForEach(x => {
                table.Rows.Add(x.ProductId, x.Name, x.ProductNumber, x.Color);
            });
            return table;
        }
    }
}