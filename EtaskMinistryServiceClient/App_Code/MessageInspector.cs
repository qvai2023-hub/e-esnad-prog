using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace ETaskServiceClient.AppCode
{
    public class MessageInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
            reply = buffer.CreateMessage();
            HttpContext.Current.Session["response"] = reply.ToString();
            //return buffer.CreateMessage().ToString();

        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {

            MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
            var xx = buffer.CreateMessage().GetReaderAtBodyContents();
            request = buffer.CreateMessage();
            HttpContext.Current.Session["request"] = request.ToString();
            return request;

            /*
        
            XmlDocument doc = new XmlDocument();
            MemoryStream ms = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(ms);
            request.WriteMessage(writer);
            writer.Flush();
            ms.Position = 0;
            doc.Load(ms);
            ChangeMessage(doc);
            ms.SetLength(0);
            writer = XmlWriter.Create(ms);
            doc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            XmlReader reader = XmlReader.Create(ms);
            request = Message.CreateMessage(reader, int.MaxValue, request.Version);
            HttpContext.Current.Session["request"] = request.ToString();
            return request;

              */

        }

        void ChangeMessage(XmlDocument doc)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            XmlNode node = doc.SelectSingleNode("//PeriodEnd");
            if (node != null)
            {

                node.InnerXml = node.InnerXml = "03/02/2014";
            }
        }

    }
}