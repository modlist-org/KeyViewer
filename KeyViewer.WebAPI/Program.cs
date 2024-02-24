using KeyViewer.WebAPI.Core.Utils;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
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

            builder.Services.AddRateLimiter(limiterOptions =>
            {
                limiterOptions.OnRejected = (ctx, cTok) =>
                {
                    if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        ctx.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                    }
                    ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return ValueTask.CompletedTask;
                };
                // �۷ι� �ӵ� ���� ����
                limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
                {
                    // ��û IPAddress
                    IPAddress? remoteIpAddress = IPAddress.Parse(context.GetIpAddress() ?? "127.0.0.1");

                    // ��û�� IPAddress�� �������� �ƴ� ���
                    if (IPAddress.IsLoopback(remoteIpAddress!) == false)
                    {
                        // IPAddress�� ���� �ӵ� ���� ����
                        return RateLimitPartition.GetSlidingWindowLimiter
                        (remoteIpAddress!, _ =>
                        new SlidingWindowRateLimiterOptions
                        {
                            // ��û ��� ���� : 100
                            PermitLimit = 3,
                            // â �̵��ð� 30��
                            Window = TimeSpan.FromSeconds(5),
                            // â ���� ���׸�Ʈ ����
                            SegmentsPerWindow = 3,  // 1���� ���׸�Ʈ : 5s / 3
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            // ���ѽ� 3���� ��û�� ��⿭�� �߰�
                            QueueLimit = 3,
                        });
                    }

                    // ������ IPAddress�� �ӵ� ���� ����.
                    return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();
            // ����Ʈ ������ ���
            app.UseRateLimiter();

            app.MapControllers();

            app.Run("http://127.0.0.1:1111");
        }
    }
}