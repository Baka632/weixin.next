using System.Threading.Tasks;
using Weixin.Next.Messaging;
using Weixin.Next.Messaging.Requests;
using Weixin.Next.Messaging.Responses;

namespace Weixin.Next.Sample.Models
{
    public class SampleMessageHandler : MessageHandler
    {
        // override ��ͬ�ķ���������ͬ����Ϣ

        protected override Task<IResponseMessage> HandleSubscribeEvent(SubscribeEventMessage message)
        {
            var scene = message.GetQrSceneValue();
            if (scene == null)
                return Empty();

            return Result(Text("��ӭɨ���ע, ��ά�볡��ֵΪ: " + scene, message));
        }

        protected override Task<IResponseMessage> HandleTextRequest(TextRequestMessage message)
        {
            var content = message.Content;

            return content.Length > 2
                ? Result(Text($"����̫��, ��Ϣû��ȫ, �ٷ�һ�ΰ�! ֻ�յ��� \"{content.Substring(content.Length / 2)}\"..", message))
                : Result(Text(content, message));
        }
    }
}