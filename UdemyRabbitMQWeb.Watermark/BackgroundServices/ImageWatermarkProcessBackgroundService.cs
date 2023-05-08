using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Text;
using System.Text.Json;
using UdemyRabbitMQWeb.Watermark.Services;

namespace UdemyRabbitMQWeb.Watermark.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        //Channel gelmesi için rabbitmqclientservice'i kullanıyoruz
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel; //constructor'da set etmeyeceğimiz için readonly demedik
        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
        }


        //StartAsync() arka plan işlemlerinin başlatılmasını sağlıyor
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //rabbit'e bağlanıyoruz
            _channel = _rabbitMQClientService.Connect();
            //Kaçar kaçar alacağız? boyutu önemli değil ve birer birer al diyoruz.
            _channel.BasicQos(0, 1, false); 

            return base.StartAsync(cancellationToken);
        }
        //Mutlaka implement edilmelidir. Belirli bir görev arka planda başlatılır ve görev tamamlandığında sonucuyla birlikte geri döner.
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Asenkron şekilde rabbitmq'dan mesajları asenkron şekilde almamızı söylüyor.
            var consumer = new AsyncEventingBasicConsumer(_channel);

            //Hangi kuyruğu okuyacağız? otomatik silinsin mi =false
            _channel.BasicConsume(RabbitMQClientService.QueueName,false,consumer);
            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        //Resime image ekleme olayı burada gerçekleştirilecektir.
        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            Task.Delay(5000).Wait();
            try 
            {
                //rabbitMQ'dan okuduğumuz mesajı arraydan dönüştürüyoruz.
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", productImageCreatedEvent.ImageName);


                var siteName = "www.mysite.com";

                //resimi alıyoruz.
                using var img = Image.FromFile(path);
                using var graphic = Graphics.FromImage(img);
                //yazacağımız yazının fontunu ayarlıyoruz
                var font = new Font(FontFamily.GenericMonospace, 40, FontStyle.Bold, GraphicsUnit.Pixel);

                //yazacağımız yazının boyutu
                var textSize = graphic.MeasureString(siteName, font);

                var color = Color.FromArgb(255, 255, 0, 0);

                var brush = new SolidBrush(color);

                var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));

                graphic.DrawString(siteName, font, brush, position);

                img.Save("wwwroot/Images/watermarks/" + productImageCreatedEvent.ImageName);
                img.Dispose();
                graphic.Dispose();
                //mesajın doğru şekilde iletildiğine dair rabbit'i bilgilendiriyoruz
                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return Task.CompletedTask;
     
        }

        //StartAsync() arka plan işlemlerinin durdurulmasını sağlıyor
        public override Task StopAsync(CancellationToken cancellationToken)
        {

            return base.StopAsync(cancellationToken);
        }
    }
}
