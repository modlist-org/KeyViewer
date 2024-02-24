using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace KeyViewer.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new BinaryInputFormatter());
            });
            builder.Services.AddRateLimiter(_ => _
            .AddSlidingWindowLimiter(policyName: "sliding", options =>
            {
                // ��û ��� ���� : 3
                options.PermitLimit = 3;
                // 10�� ���� �ִ� PermitLimit���� ��û�� ó�� ����
                options.Window = TimeSpan.FromSeconds(10);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                // ���ѽ� 5���� ��û�� ��⿭�� �߰�
                options.QueueLimit = 5;
            }));

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();

            app.Run("http://127.0.0.1:1111");
        }
    }
}