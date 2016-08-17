using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Weixin.Next.Api
{
    // ReSharper disable InconsistentNaming
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

        /// <summary>
        /// ���� jsapi �е� wx.config ������Ҫ�Ĳ���
        /// </summary>
        /// <param name="ticketManager">jsapi-ticket �Ĺ�����</param>
        /// <param name="appId">΢��Ӧ��Ψһ��ʶ</param>
        /// <param name="url">��Ҫ���� wx.config ҳ���������ַ</param>
        /// <param name="jsApiList">��Ҫ�Ľӿ������б�</param>
        /// <param name="debug">ֻ��������ģʽ</param>
        /// <returns></returns>
        public static async Task<JsConfig> Generate(IJsapiTicketManager ticketManager, string appId, string url, string[] jsApiList, bool debug = false)
        {
            var hashIndex = url.IndexOf('#');
            if (hashIndex > 0)
                url = url.Substring(0, hashIndex);

            var result = new JsConfig
            {
                debug = debug,
                appId = appId,
                timestamp = DateTime.UtcNow.ToUnixTimestamp(),
                jsApiList = jsApiList,
                nonceStr = Guid.NewGuid().ToString("n"),
            };

            var ticket = await ticketManager.GetTicket().ConfigureAwait(false);

            var string1 = $"jsapi_ticket={ticket}&noncestr={result.nonceStr}&timestamp={result.timestamp:D}&url={url}";

            using (var sha1 = SHA1.Create())
            {
                var signatureData = sha1.ComputeHash(Encoding.UTF8.GetBytes(string1));
                result.signature = string.Concat(signatureData.Select(x => x.ToString("x2")));
            }

            return result;
        }
    }
}