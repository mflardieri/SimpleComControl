using MauiChatApp.Core.Bases;
using MauiChatApp.Core.Enums;

namespace MauiChatApp.Core.Models
{
    public class MessageIndentityRequest : MessageRequest
    {
        //For Indentication this request only cares about Ids.
        //Certain Inquiry Types will care if the Indentity on the Request is set or not.
        public ChatIndentity Current { get; set; }
        public MessageIndentityInquiryType InquiryType { get; set; }
    }
}
