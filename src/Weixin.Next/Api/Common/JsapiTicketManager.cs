using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weixin.Next.Api
{
    public interface IJsapiTicketManager
    {
        /// <summary>
        /// ��ȡ jsapi_ticket ������ʱ��
        /// </summary>
        /// <param name="forceRefresh">�Ƿ���δ������ʱ��ʱǿ��ˢ��</param>
        /// <returns></returns>
        Task<JsapiTicketInfo> GetTicketInfo(bool forceRefresh = false);
    }

    public class JsapiTicketInfo
    {
        /// <summary>
        /// jsapi_ticket
        /// </summary>
        public string Ticket { get; set; }
        /// <summary>
        /// ����ʱ��, unix ʱ���
        /// </summary>
        public long ExpireTime { get; set; }
    }

    public static class JsapiTicketManagerExtensions
    {
        /// <summary>
        /// ��ȡ jsapi_ticket
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="forceRefresh">�Ƿ���δ������ʱ��ʱǿ��ˢ��</param>
        /// <returns></returns>
        public static async Task<string> GetTicket(this IJsapiTicketManager manager, bool forceRefresh = false)
        {
            var info = await manager.GetTicketInfo(forceRefresh).ConfigureAwait(false);
            return info.Ticket;
        }
    }

    public abstract class JsapiTicketManagerBase : IJsapiTicketManager
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private string _ticket;
        private long _expireTime;

        public async Task<JsapiTicketInfo> GetTicketInfo(bool forceRefresh = false)
        {
            var expireTime = Interlocked.Read(ref _expireTime);
            var now = DateTime.UtcNow.Ticks;
            if (forceRefresh || expireTime <= now)
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                try
                {
                    expireTime = Volatile.Read(ref _expireTime);
                    now = DateTime.UtcNow.Ticks;
                    if (forceRefresh || expireTime <= now)
                    {
                        var result = await Refresh().ConfigureAwait(false);
                        _ticket = result.Ticket;
                        expireTime = result.ExpireTime;
                        Interlocked.Exchange(ref _expireTime, expireTime);

                        return result;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return new JsapiTicketInfo
            {
                Ticket = _ticket,
                ExpireTime = _expireTime,
            };
        }

        protected abstract Task<JsapiTicketInfo> Refresh();
    }

    /// <summary>
    /// ���ڻ��沢����ʱˢ�� jsapi_ticket
    /// </summary>
    public class JsapiTicketManager : JsapiTicketManagerBase
    {
        private readonly ApiConfig _config;

        public JsapiTicketManager(ApiConfig config)
        {
            _config = config;
        }

        protected override async Task<JsapiTicketInfo> Refresh()
        {
            var result = await Ticket.GetJsapi(_config);
            return new JsapiTicketInfo
            {
                Ticket = result.ticket,
                //��ǰ 10 �뵽��
                ExpireTime = DateTime.UtcNow.AddSeconds(result.expires_in - 10).Ticks,
            };
        }
    }

}