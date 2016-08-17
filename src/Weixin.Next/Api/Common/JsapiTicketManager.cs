using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weixin.Next.Api
{
    /// <summary>
    /// ���ڻ��沢����ʱˢ�� jsapi_ticket
    /// </summary>
    public class JsapiTicketManager
    {
        private readonly string _appId;
        private readonly ApiConfig _config;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private string _ticket;
        private long _expireTime;

        public JsapiTicketManager(string appId, ApiConfig config)
        {
            _appId = appId;
            _config = config;
        }

        /// <summary>
        /// ��ȡ jsapi_ticket
        /// </summary>
        /// <param name="forceRefresh">�Ƿ���δ������ʱ��ʱǿ��ˢ��</param>
        /// <returns></returns>
        public async Task<string> GetTicket(bool forceRefresh = false)
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
                        var result = await Ticket.GetJsapi(_config).ConfigureAwait(false);
                        _ticket = result.ticket;
                        //��ǰ 10 �뵽��
                        expireTime = now + TimeSpan.FromSeconds(result.expires_in - 10).Ticks;
                        Interlocked.Exchange(ref _expireTime, expireTime);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return _ticket;
        }

        /// <summary>
        /// ���� jsapi �е� wx.config ������Ҫ�Ĳ���
        /// </summary>
        /// <param name="url">��Ҫ���� wx.config ҳ���������ַ</param>
        /// <param name="jsApiList">��Ҫ�Ľӿ������б�</param>
        /// <param name="debug">ֻ��������ģʽ</param>
        /// <returns></returns>
        public async Task<JsConfig> GenerateJsConfig(string url, string[] jsApiList, bool debug = false)
        {
            var hashIndex = url.IndexOf('#');
            if (hashIndex > 0)
                url = url.Substring(0, hashIndex);

            var result = new JsConfig
            {
                debug = debug,
                appId = _appId,
                timestamp = DateTime.UtcNow.ToUnixTimestamp(),
                jsApiList = jsApiList,
                nonceStr = Guid.NewGuid().ToString("n"),
            };

            var ticket = await GetTicket().ConfigureAwait(false);

            var string1 = $"jsapi_ticket={ticket}&noncestr={result.nonceStr}&timestamp={result.timestamp:D}&url={url}";

            using (var sha1 = SHA1.Create())
            {
                var signatureData = sha1.ComputeHash(Encoding.UTF8.GetBytes(string1));
                result.signature = string.Concat(signatureData.Select(x => x.ToString("x2")));
            }

            return result;
        }
    }

    public class JsConfig
    {
        // ��������ģʽ,���õ�����api�ķ���ֵ���ڿͻ���alert��������Ҫ�鿴����Ĳ�����������pc�˴򿪣�������Ϣ��ͨ��log���������pc��ʱ�Ż��ӡ
        public bool debug { get; set; }
        // ������ںŵ�Ψһ��ʶ
        public string appId { get; set; }
        // �������ǩ����ʱ���
        public long timestamp { get; set; }
        // �������ǩ���������
        public string nonceStr { get; set; }
        // ���ǩ��
        public string signature { get; set; }
        // �����Ҫʹ�õ�JS�ӿ��б�
        public string[] jsApiList { get; set; }
    }
}