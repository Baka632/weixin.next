namespace Weixin.Next.Messaging.Responses
{
    public interface IResponseMessage
    {
        /// <summary>
        /// ת��Ϊ����΢�ŷ���������Ϣ�ı�
        /// </summary>
        /// <returns></returns>
        string Serialize();
    }

    /// <summary>
    /// ������Ҫֱ�ӷ����ַ���, �����Ǹ�ʽ���� xml ��Ϣ
    /// </summary>
    public class RawResponseMessage : IResponseMessage
    {
        private readonly string _message;

        public RawResponseMessage(string message)
        {
            _message = message;
        }

        /// <summary>
        /// ����ֱ�ӷ��ص��ַ���
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

        public string Serialize()
        {
            return Message;
        }
    }
}